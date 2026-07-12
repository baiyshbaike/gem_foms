using System.ComponentModel.DataAnnotations;

namespace Contracts.MedCenterMachines;

public sealed class UpdateMedCenterMachineRequest
{
    [EnumDataType(typeof(MachineAcquisitionTypeDto))]
    public MachineAcquisitionTypeDto AcquisitionType { get; set; } = MachineAcquisitionTypeDto.New;

    [Required]
    [MaxLength(50)]
    public string InventoryNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Model { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string SerialNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Manufacturer { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ManufacturingCountry { get; set; }

    [Range(1980, 2100)]
    public int ManufactureYear { get; set; }

    [MaxLength(200)]
    public string? CertificateHolder { get; set; }

    [MaxLength(100)]
    public string? CertificateHolderCountry { get; set; }

    [MaxLength(100)]
    public string? CertificateNumber { get; set; }

    [MaxLength(100)]
    public string? CertificateCountry { get; set; }

    [Required]
    public DateOnly? CertificateIssuedAt { get; set; }

    [MaxLength(200)]
    public string? PermitName { get; set; }

    [MaxLength(100)]
    public string? PermitNumber { get; set; }

    [MaxLength(100)]
    public string? PermitSeries { get; set; }

    [Required]
    public DateOnly? PermitExpiresAt { get; set; }

    [Range(1, 30)]
    public int DailySessionLimit { get; set; }

    [Range(1, 1440)]
    public int BetweenSessionCooldownMinutes { get; set; }

    [Range(1, 1440)]
    public int DailyLimitCooldownMinutes { get; set; }

    public bool IsApproved { get; set; } = true;
    public bool IsActive { get; set; } = true;
}