using System;
namespace Dialysis.Shared.Models
{
    public class FirstCardiovascular
    {
        public long Id { get; set; }
        public long MedCardId { get; set; }
        public string? Perekardilnaia { get; set; }
        public string? Epigastralnaia { get; set; }
        public string? Vidimye { get; set; }
        public string? Prochee { get; set; }
        public string? Verhushechnaia { get; set; }
        public string? Drojanie { get; set; }
        public string? Prochee2 { get; set; }
        public string? Granitsa { get; set; }
        public string? Sosudistiy { get; set; }
        public string? Prochee3 { get; set; }
        public string? Ton { get; set; }
        public string? Shum { get; set; }
        public string? Prochee4 { get; set; }
        public string? Prochee5 { get; set; }
        public string? Ad { get; set; }
        public string? AdChastota { get; set; }
        public string? AdRitm { get; set; }
        public string? AdDefisit { get; set; }
        public string? AdProchee { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public long? LastModifiedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public string? UniqId { get; set; }
    }
}

