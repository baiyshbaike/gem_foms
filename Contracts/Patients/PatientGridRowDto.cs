namespace Contracts.Patients;

public sealed class PatientGridRowDto
{
    public long Id { get; init; }
    public string Inn { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string MiddleName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public DateOnly BirthDate { get; init; }
    public PatientGenderDto Gender { get; init; }
    public string Address { get; init; } = string.Empty;
    public string Address2 { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public long RegionId { get; init; }
    public string RegionName { get; init; } = string.Empty;
    public long DistrictId { get; init; }
    public string DistrictName { get; init; } = string.Empty;
    public long GroupId { get; init; }
    public string GroupCode { get; init; } = string.Empty;
    public string GroupName { get; init; } = string.Empty;
    public bool SpecialStatus { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public bool IsActive { get; init; }
}
