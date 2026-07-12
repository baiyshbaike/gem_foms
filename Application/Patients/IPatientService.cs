using Contracts.Patients;

namespace Application.Patients;

public interface IPatientService
{
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

    Task<PatientDto?> CreateAsync(
        long userId,
        CreatePatientRequest request,
        CancellationToken cancellationToken);

    Task<PatientDto?> UpdateAsync(
        long id,
        long userId,
        UpdatePatientRequest request,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(
        long id,
        long userId,
        CancellationToken cancellationToken);
}