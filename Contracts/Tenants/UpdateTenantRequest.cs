namespace Contracts.Tenants;

public sealed class UpdateTenantRequest : CreateTenantRequest
{
    public bool IsActive { get; set; }
}
