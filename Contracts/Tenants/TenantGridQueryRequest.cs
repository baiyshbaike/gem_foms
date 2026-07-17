using System.ComponentModel.DataAnnotations;

namespace Contracts.Tenants;

public class TenantGridQueryRequest
{
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 25;

    [MaxLength(200)]
    public string? Search { get; set; }

    [MaxLength(10)]
    public TenantGridSortDto[] Sorting { get; set; } = [];

    [MaxLength(20)]
    public TenantGridFilterDto[] Filters { get; set; } = [];

    [MaxLength(50)]
    public string? GroupBy { get; set; }
}

public sealed class TenantGridSortDto
{
    [Required]
    [MaxLength(50)]
    public string Field { get; set; } = string.Empty;

    public bool Descending { get; set; }
}

public sealed class TenantGridFilterDto
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
