using System.ComponentModel.DataAnnotations;

namespace Contracts.Tenants;

public class CreateTenantRequest
{
    [Required]
    [MaxLength(100)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Address { get; set; }

    [Required]
    [MaxLength(50)]
    public string Phone { get; set; } = string.Empty;

    [Range(1, long.MaxValue)]
    public long RegionId { get; set; }

    [Range(1, long.MaxValue)]
    public long DistrictId { get; set; }
}
