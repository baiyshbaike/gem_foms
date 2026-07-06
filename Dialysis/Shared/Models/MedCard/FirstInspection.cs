using System;
namespace Dialysis.Shared.Models
{
    public class FirstInspection
    {
        public long Id { get; set; }
        public long MedCardId { get; set; }
        public string? Complaints { get; set; }
        public string? GeneralState { get; set; }
        public string? DeformationOfBones { get; set; }
        public string? КednessOfJoints { get; set; }
        public string? Musculature { get; set; }
        public string? PainMuscles { get; set; }
        public string? Other { get; set; }
        public string? Anamnez { get; set; }
        public string? AnamnezLive { get; set; }
        public string? Inn { get; set; }

        public double? Rost { get; set; }
        public double? Ves { get; set; }
        public string? Grudnoi { get; set; }
        public string? Jivot { get; set; }
        public string? Kozhnye { get; set; }
        public string? Tela { get; set; }
        public string? Svet { get; set; }
        public string? Suhost { get; set; }
        public string? Uprugost { get; set; }
        public string? Syp { get; set; }
        public string? Oteki { get; set; }
        public string? Limfaticheskiy { get; set; }
        public string? Deformatsia { get; set; }
        public string? Pripuhanie { get; set; }
        public string? Muskulatura { get; set; }
        public string? Boli { get; set; }

        public string? BodyT { get; set; }
        public string? Pigmentation { get; set; }
        public string? Scars{ get; set; }
        public string? Bedsores { get; set; }
        public string? Damage { get; set; }
        public string? Dermographism { get; set; }


        public long? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public long? LastModifiedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public string? UniqId { get; set; }

    }
    
}

