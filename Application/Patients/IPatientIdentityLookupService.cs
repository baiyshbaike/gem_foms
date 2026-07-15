using Contracts.Patients;

namespace Application.Patients;

public interface IPatientIdentityLookupService
{
    Task<PatientIdentityLookupDto> LookupAsync(
        string inn,
        CancellationToken cancellationToken);
}
