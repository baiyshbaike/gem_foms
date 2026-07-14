namespace Contracts.Patients;

public sealed record PatientGroupDto(
    long Id,
    string Code,
    string Name);
