using System.Diagnostics;
using System.IO;
using System.Security.Claims;
using System.Text;
using Dialysis.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Dialysis.Server.Domain.Services
{
    public class HttpLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILoggerManager _logger;

        public HttpLoggingMiddleware(RequestDelegate next, ILoggerManager logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context, ILogSettingsProvider settingsProvider, AppDbContext dbContext)
        {
            // Skip logging for audit paths and static files to prevent recursion and performance issues
            var path = context.Request.Path.Value?.ToLower() ?? "";
            
            if (path.Contains("/audit/") || 
                path.EndsWith(".js") || path.EndsWith(".css") || 
                path.EndsWith(".png") || path.EndsWith(".jpg") || 
                path.Contains("/_framework/"))
            {
                await _next(context);
                return;
            }

            var settings = await settingsProvider.GetSettingsAsync();
            if (settings == null || !settings.LogHttpEnabled)
            {
                await _next(context);
                return;
            }

            var method = context.Request.Method;
            var allowedMethods = settings.LoggedMethods?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                ?? Array.Empty<string>();

            if (allowedMethods.Length > 0 && !allowedMethods.Contains(method, StringComparer.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            var stopwatch = Stopwatch.StartNew();

            var log = new HttpLog
            {
                Timestamp = DateTime.Now,
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                HttpMethod = method,
                Path = context.Request.Path,
                QueryString = context.Request.QueryString.ToString(),
                UserId = GetUserId(context),
                UserName = context.User?.Identity?.Name
            };

            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            var requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            if (requestBody.Length > settings.MaxBodyLength)
                requestBody = requestBody[..settings.MaxBodyLength];

            log.RequestBody = requestBody;
            log.RequestHeaders = System.Text.Json.JsonSerializer.Serialize(
                context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()));

            var originalResponseBody = context.Response.Body;
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                log.DurationMs = stopwatch.ElapsedMilliseconds;
                log.StatusCode = context.Response.StatusCode;

                responseBodyStream.Position = 0;
                using var responseReader = new StreamReader(responseBodyStream, Encoding.UTF8);
                var responseBody = await responseReader.ReadToEndAsync();

                if (responseBody.Length > settings.MaxBodyLength)
                    responseBody = responseBody[..settings.MaxBodyLength];

                log.ResponseBody = responseBody;
                log.ResponseHeaders = System.Text.Json.JsonSerializer.Serialize(
                    context.Response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()));

                responseBodyStream.Position = 0;
                await responseBodyStream.CopyToAsync(originalResponseBody);

                try
                {
                    dbContext.Set<HttpLog>().Add(log);
                    await dbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to save HTTP log: {ex.Message}");
                }
            }
        }

        private static long? GetUserId(HttpContext context)
        {
            var claim = context.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null && long.TryParse(claim.Value, out var userId))
                return userId;
            return null;
        }
    }
}
