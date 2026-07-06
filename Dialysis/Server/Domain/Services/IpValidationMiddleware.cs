using System.Security.Claims;

namespace Dialysis.Server.Domain.Services;

/// <summary>
/// Middleware to validate that the request IP matches the IP stored in the JWT token.
/// Prevents token replay attacks from different IP addresses.
/// </summary>
public class IpValidationMiddleware
{
    private readonly RequestDelegate _next;

    public IpValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tokenIp = context.User.FindFirstValue("ip");
            var requestIp = context.Connection.RemoteIpAddress?.ToString();

            if (!string.IsNullOrEmpty(tokenIp) && tokenIp != requestIp)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    succeeded = false,
                    messages = new[] { "Сессия недействительна. Пожалуйста, войдите заново." }
                });
                return;
            }
        }

        await _next(context);
    }
}
