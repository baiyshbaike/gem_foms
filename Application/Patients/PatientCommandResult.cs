namespace Application.Patients;

public sealed record PatientCommandResult<T>(
    PatientCommandStatus Status,
    T? Value = default);
