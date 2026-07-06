using System;
namespace Dialysis.Shared.Models
{
    public class FirstConfectionery
    {
        public long Id { get; set; }
        public long MedCardId { get; set; }
        public string? Slizistaia { get; set; }
        public string? Polost { get; set; }
        public string? Yazyk { get; set; }
        public string? Zev { get; set; }
        public string? Osmotr { get; set; }
        public string? Jivot { get; set; }
        public string? Perkussia { get; set; }
        public string? Prochee { get; set; }
        public string? Prochee2 { get; set; }
        public string? Palypatsia { get; set; }
        public string? Granitsa { get; set; }
        public string? Jelchniy { get; set; }
        public string? PodJelchniy { get; set; }
        public string? Selezenka { get; set; }
        public string? Stul { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public long? LastModifiedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public string? UniqId { get; set; }

    }
}

