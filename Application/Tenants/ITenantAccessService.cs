using Contracts.Tenants;

namespace Application.Tenants;

public sealed record TenantFilterResult(
    bool Succeeded,
    IReadOnlyList<string> TenantIds);

public interface ITenantAccessService
{
    Task<IReadOnlyList<TenantDto>> GetAccessibleTenantsAsync(
        long userId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> GetAccessibleTenantIdsAsync(
        long userId,
        CancellationToken cancellationToken);

    Task<bool> CanAccessTenantAsync(
        long userId,
        string tenantId,
        CancellationToken cancellationToken);

    Task<TenantFilterResult> ResolveTenantFilterAsync(
        long userId,
        IReadOnlyList<string>? requestedTenantIds,
        CancellationToken cancellationToken);
}