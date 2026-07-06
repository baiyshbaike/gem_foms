using System;
namespace Dialysis.Shared.Models
{
    public class FirstNeuro
    {
        public long Id { get; set; }
        public long MedCardId { get; set; }
        public string? Soznanye { get; set; }
        public string? Rech { get; set; }
        public string? Pohodka { get; set; }
        public string? Golovokrujenie { get; set; }
        public string? Litso { get; set; }
        public string? Glaznye { get; set; }
        public string? Zrachki { get; set; }
        public string? Dvijenie { get; set; }
        public string? Glotochnyi { get; set; }
        public string? Yazyk { get; set; }
        public string? Poverhnostnye { get; set; }
        public string? Potologicheskie { get; set; }
        public DateTime? Date1 { get; set; }
        public string? Sindrom { get; set; }
        public string? Meningealnye { get; set; }
        public string? Kerniga { get; set; }
        public string? Romberga { get; set; }
        public DateTime? Date2 { get; set; }
        public string? Intellect { get; set; }
        public string? Pamiat { get; set; }
        public string? Mnitelnost { get; set; }
        public string? Vnushaemost { get; set; }
        public string? StatusOfArteriovenous { get; set; }

        public long? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public long? LastModifiedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }

        public string? UniqId { get; set; }

    }
}

