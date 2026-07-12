using System.ComponentModel.DataAnnotations;

namespace Contracts.Patients;

public sealed class CreatePatientRequest
{
    [Required]
    [StringLength(14, MinimumLength = 14)]
    [RegularExpression(@"^\d{14}$")]
    public string Inn { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string MiddleName { get; set; } = string.Empty;

    [Required]
    public DateOnly? BirthDate { get; set; }

    [Range(1, 2)]
    public PatientGenderDto Gender { get; set; }

    [Required]
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Address2 { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Phone { get; set; } = string.Empty;

    [Range(1, long.MaxValue)]
    public long DistrictId { get; set; }

    [Range(1, long.MaxValue)]
    public long RegionId { get; set; }

    public bool SpecialStatus { get; set; } = false;
}