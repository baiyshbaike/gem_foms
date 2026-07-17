using Contracts.Tenants;
using Domain.Users;

namespace Application.Tenants;

public interface ITenantService
{
    Task<TenantGridQueryResult> QueryGridAsync(
        long userId,
        TenantGridQueryRequest request,
        CancellationToken cancellationToken);

    Task<TenantGridQueryResult> ExportGridAsync(
        long userId,
        TenantGridExportRequest request,
        CancellationToken cancellationToken);

    Task<TenantDetailsDto?> GetByIdAsync(
        long userId,
        string tenantId,
        CancellationToken cancellationToken);

    Task<TenantCommandResult<TenantDetailsDto>> CreateAsync(
        long userId,
        CreateTenantRequest request,
        CancellationToken cancellationToken);

    Task<TenantCommandResult<TenantDetailsDto>> UpdateAsync(
        long userId,
        string tenantId,
        UpdateTenantRequest request,
        CancellationToken cancellationToken);

    Task<TenantCommandResult<TenantDetailsDto>> DeactivateAsync(
        long userId,
        string tenantId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<TenantDto>> GetMyTenantsAsync(long userId, CancellationToken cancellationToken);
    Task<SwitchTenantResponse?> SwitchAsync(long userId, string tenantId, CancellationToken cancellationToken);
}
