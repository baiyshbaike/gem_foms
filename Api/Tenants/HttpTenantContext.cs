using Application.Tenants;

namespace Api.Tenants;

public sealed class HttpTenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpTenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? TenantId =>
        _httpContextAccessor.HttpContext?.User.FindFirst("active_tenant")?.Value;

    public string RequiredTenantId =>
        TenantId ?? throw new InvalidOperationException("Active tenant is required.");
}