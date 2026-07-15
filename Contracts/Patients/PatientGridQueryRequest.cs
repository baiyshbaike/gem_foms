using System.ComponentModel.DataAnnotations;

namespace Contracts.Patients;

public class PatientGridQueryRequest
{
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 25;

    [MaxLength(200)]
    public string? Search { get; set; }

    [MaxLength(10)]
    public PatientGridSortDto[] Sorting { get; set; } = [];

    [MaxLength(20)]
    public PatientGridFilterDto[] Filters { get; set; } = [];

    [MaxLength(50)]
    public string? GroupBy { get; set; }
}

public sealed class PatientGridSortDto
{
    [Required]
    [MaxLength(50)]
    public string Field { get; set; } = string.Empty;

    public bool Descending { get; set; }
}

public sealed class PatientGridFilterDto
{
    [Required]
    [MaxLength(50)]
    public string Field { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string Operator { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Value { get; set; }

    [MaxLength(500)]
    public string? ValueTo { get; set; }
}
