namespace Contracts.Patients;

public sealed record PatientGridLoadResult(
    object Data,
    int TotalCount,
    int GroupCount,
    object?[]? Summary);
