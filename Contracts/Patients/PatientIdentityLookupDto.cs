namespace Contracts.Patients;

public sealed record PatientIdentityLookupDto(
    bool Found,
    string? FirstName,
    string? LastName,
    string? MiddleName);
