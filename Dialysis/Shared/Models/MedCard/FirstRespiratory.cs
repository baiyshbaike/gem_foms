using System;
namespace Dialysis.Shared.Models
{
    public class FirstRespiratory
    {
        public long Id { get; set; }
        public long MedCardId { get; set; }
        public string? Nosovoe { get; set; }
        public string? Grudnaia { get; set; }
        public string? Tip { get; set; }
        public string? Grudnoi { get; set; }
        public string? Glubina { get; set; }
        public string? Chastota { get; set; }
        public string? Golosovoe { get; set; }
        public string? Sravnitelnaia { get; set; }
        public string? Topograficheskaia { get; set; }
        public string? Dyhatelnye { get; set; }
        public string? Hrip { get; set; }
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

