namespace Contracts.Patients;

public sealed record PatientGridQueryResult(
    IReadOnlyList<PatientGridRowDto> Items,
    int TotalCount,
    IReadOnlyList<PatientGridGroupSummaryDto> Groups);

public sealed record PatientGridGroupSummaryDto(
    string Key,
    string Label,
    int Count);
