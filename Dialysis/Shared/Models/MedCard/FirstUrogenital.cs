using System;
namespace Dialysis.Shared.Models
{
    public class FirstUrogenital
    {
        public long Id { get; set; }
        public long MedCardId { get; set; }
        public string? Chastota { get; set; }
        public string? Sutochnoe { get; set; }
        public string? Rezi { get; set; }
        public string? Nederjanie { get; set; }
        public string? SvetMochi { get; set; }
        public string? PochkiPalpatsia { get; set; }
        public string? PochkiSimptom { get; set; }
        public string? PochkiProchee { get; set; }
        public string? MochevoiPalpatsia { get; set; }
        public string? MochevoiPerkissia { get; set; }
        public string? MochevoiProchee { get; set; }
        public string? Prochee { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public long? LastModifiedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public string? UniqId { get; set; }

    }
}

