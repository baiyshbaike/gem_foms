using Domain.Common;
using Domain.Sessions;

namespace Domain.MedCenters;

public sealed class MedCenterMachine : TenantActiveSoftDeletableAuditableEntityBase
{
    public MachineAcquisitionType AcquisitionType { get; set; } = MachineAcquisitionType.New;

    public string InventoryNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;

    public string Manufacturer { get; set; } = string.Empty;
    public string? ManufacturingCountry { get; set; }
    public int ManufactureYear { get; set; }

    public string? CertificateHolder { get; set; }
    public string? CertificateHolderCountry { get; set; }
    public string? CertificateNumber { get; set; }
    public string? CertificateCountry { get; set; }
    public DateOnly CertificateIssuedAt { get; set; }

    public string? PermitName { get; set; }
    public string? PermitNumber { get; set; }
    public string? PermitSeries { get; set; }
    public DateOnly PermitExpiresAt { get; set; }

    public int DailySessionLimit { get; set; }
    public int BetweenSessionCooldownMinutes { get; set; }
    public int DailyLimitCooldownMinutes { get; set; }

    public bool IsApproved { get; set; } = true;

    public ICollection<HdSession> Sessions { get; set; } = new List<HdSession>();
}