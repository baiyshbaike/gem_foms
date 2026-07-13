namespace Contracts.Patients;

public sealed record PatientDto(
    long Id,
    string Inn,
    string FirstName,
    string LastName,
    string MiddleName,
    DateOnly BirthDate,
    PatientGenderDto Gender,
    string Address,
    string Address2,
    string Phone,
    long DistrictId,
    long RegionId,
    long GroupId,
    string GroupCode,
    string GroupName,
    bool SpecialStatus,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    bool IsActive);
