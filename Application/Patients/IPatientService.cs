using Contracts.Patients;

namespace Application.Patients;

public interface IPatientService
{
    Task<PatientGridQueryResult> QueryGridAsync(
        PatientGridQueryRequest request,
        CancellationToken cancellationToken);

    Task<PatientGridQueryResult> ExportGridAsync(
        PatientGridExportRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<PatientGroupDto>> GetGroupsAsync(
        CancellationToken cancellationToken);

    Task<IReadOnlyList<PatientDto>> GetAsync(
        string? search,
        long? groupId,
        CancellationToken cancellationToken);

    Task<PatientDto?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken);

    Task<PatientDto?> GetByInnAsync(
        string inn,
        CancellationToken cancellationToken);

    Task<PatientCommandResult<PatientDto>> CreateAsync(
        long userId,
        CreatePatientRequest request,
        CancellationToken cancellationToken);

    Task<PatientCommandResult<PatientDto>> UpdateAsync(
        long id,
        long userId,
        UpdatePatientRequest request,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(
        long id,
        long userId,
        CancellationToken cancellationToken);
}
