using System;
namespace Dialysis.Shared.Models
{
    public class FirstEndocrine
    {
        public long Id { get; set; }
        public long MedCardId { get; set; }
        public string? Shitovidnaia { get; set; }
        public string? Glaznaia { get; set; }
        public string? Potootdelenie { get; set; }
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

