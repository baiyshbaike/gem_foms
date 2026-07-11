namespace Application.Tenants;

public interface ITenantContext
{
    string? TenantId { get; }
    string RequiredTenantId { get; }
}
