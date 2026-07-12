namespace Application.MedCenterMachines;

public sealed record MedCenterMachineCommandResult<T>(
    MedCenterMachineCommandStatus Status,
    T? Value = default);