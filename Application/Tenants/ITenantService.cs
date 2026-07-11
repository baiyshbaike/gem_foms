using Contracts.Tenants;
using Domain.Users;

namespace Application.Tenants;

public interface ITenantService
{
    Task<IReadOnlyList<TenantDto>> GetMyTenantsAsync(long userId, CancellationToken cancellationToken);
    Task<SwitchTenantResponse?> SwitchAsync(long userId, string tenantId, CancellationToken cancellationToken);
}