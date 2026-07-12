using Contracts.MedCenterMachines;

namespace Application.MedCenterMachines;

public interface IMedCenterMachineService
{
    Task<IReadOnlyList<MedCenterMachineDto>?> GetAsync(
        long userId,
        IReadOnlyList<string>? tenantIds,
        CancellationToken cancellationToken);

    Task<MedCenterMachineDto?> GetByIdAsync(
        long userId,
        string tenantId,
        long id,
        CancellationToken cancellationToken);

    Task<MedCenterMachineCommandResult<MedCenterMachineDto>> CreateAsync(
        string tenantId,
        long userId,
        CreateMedCenterMachineRequest request,
        CancellationToken cancellationToken);

    Task<MedCenterMachineCommandResult<MedCenterMachineDto>> UpdateAsync(
        string tenantId,
        long id,
        long userId,
        UpdateMedCenterMachineRequest request,
        CancellationToken cancellationToken);

    Task<MedCenterMachineCommandResult<bool>> DeleteAsync(
        string tenantId,
        long id,
        long userId,
        CancellationToken cancellationToken);
}