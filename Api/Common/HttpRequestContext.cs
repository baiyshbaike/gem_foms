using System.Security.Claims;
using Application.Common;

namespace Api.Common;

public sealed class HttpRequestContext : IRequestContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpRequestContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public long? UserId
    {
        get
        {
            var sub = HttpContext?.User.FindFirst("sub")?.Value;
            return long.TryParse(sub, out var userId)? userId: null;
        }
    }
    public string? Username =>
        HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

    public string? IpAddress =>
        HttpContext?.Connection.RemoteIpAddress?.ToString();

    public string? UserAgent =>
        HttpContext?.Request.Headers.UserAgent.ToString();

    public string? HttpMethod =>
        HttpContext?.Request.Method;

    public string? Path =>
        HttpContext?.Request.Path.Value;

    public string CorrelationId =>
        HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString("N");

    private HttpContext? HttpContext => _httpContextAccessor.HttpContext;
}