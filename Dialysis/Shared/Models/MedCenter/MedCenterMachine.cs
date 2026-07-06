using System;
namespace Dialysis.Shared.Models
{
	public class MedCenterMachine
	{
        public long Id { get; set; }
        public long MedCenterId { get; set; }
        public string? Name { get; set; }
        public string? Model { get; set; }
        public string? Number { get; set; }
        public string? Manufacturer { get; set; }
        public string? ManCountry { get; set; }
        public DateTime? ManDate { get; set; }
        public string? LicenseCountry { get; set; }
        public string? CertificateHolder { get; set; }
        public string? CertificateHolderCountry { get; set; }
        public string? CertificateNumber { get; set; }
        public string? CertificateCountry { get; set; }
        public DateTime? CertificateDate { get; set; }
        public string? CertificateMedCenterId { get; set; }
        public string? PermitName { get; set; }
        public string? PermitNumber { get; set; }
        public string? PermitSeria { get; set; }
        public DateTime? PermitDate { get; set; }
        public long? TotalSessions { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public long? LastModifiedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public bool IsActive { get; set; }
        public bool IsApproved { get; set; } = true;
        public long? Status { get; set; } = 1;
        public string? UpFile1 { get; set; }
        public string? UpFile2 { get; set; }
        public string? UpFile3 { get; set; }
    }
}

