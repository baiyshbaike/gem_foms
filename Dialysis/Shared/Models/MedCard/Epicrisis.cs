using System;
namespace Dialysis.Shared.Models
{
	public class Epicrisis
    {
        public long Id { get; set; }
        public long PatientId { get; set; }
        public long MedCardId { get; set; }
        public DateTime? Started { get; set; }
        public string? Osn { get; set; }
        public string? Osl { get; set; }
        public string? Sop { get; set; }
        public string? MedHelpType { get; set; }
        public string? Diagnosis { get; set; }
        public string? Status { get; set; }
        public DateTime? DateVgv { get; set; }
        public DateTime? DateVgs { get; set; }
        public string? TotalHD { get; set; }
        public string? TotalHDF { get; set; }
        public string? MochevinaDo { get; set; }
        public string? MochevinaAfter { get; set; }
        public string? AzotDo { get; set; }
        public string? AzotAfter { get; set; }
        public string? KreatininDo { get; set; }
        public string? KreatininAfter { get; set; }
        public string? NaDo { get; set; }
        public string? NaAfter { get; set; }
        public string? KDo { get; set; }
        public string? KAfter { get; set; }
        public string? CaDo { get; set; }
        public string? CaAfter { get; set; }
        public string? Fosfor { get; set; }
        public string? Fe { get; set; }
        public string? URR { get; set; }
        public string? Imt { get; set; }
        public string? Ad { get; set; }
        public string? StartWeight { get; set; }
        public string? StartHeight { get; set; }
        public string? Rw { get; set; }
        public string? GruppaKrovi { get; set; }
        public string? Gemotransfuzii { get; set; }
        public string? Eritropoetin { get; set; }
        public string? PreparatFe { get; set; }
        public string? Ng { get; set; }
        public string? Ht { get; set; }
        public string? Erty { get; set; }
        public string? Tromb { get; set; }
        public string? Leykty { get; set; }
        public string? Pya { get; set; }
        public string? Sya { get; set; }
        public string? E { get; set; }
        public string? M { get; set; }
        public string? L { get; set; }
        public string? Soe { get; set; }
        public string? Bilirubin { get; set; }
        public string? ObshiyBelok { get; set; }
        public string? ObshiyXC { get; set; }
        public string? SaharKrovi { get; set; }
        public string? Alt { get; set; }
        public string? Ast { get; set; }
        public string? Achtv { get; set; }
        public string? Pti { get; set; }
        public string? Uf { get; set; }
        public string? RazmerFiltra { get; set; }
        public string? Gipotenzia { get; set; }
        public string? Gipertenzia { get; set; }
        public string? Sudorogi { get; set; }
        public string? Shok { get; set; }
        public string? GolovnoiBol { get; set; }
        public string? Toshnota { get; set; }
        public string? Lihorodka { get; set; }
        public string? TrombABfist { get; set; }
        public string? Days { get; set; }
        public string? Preparatov { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public long? LastModifiedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }        
        public long? GlobalStatusId { get; set; }
        public string? ActFile { get; set; }
        //public string? UniqId { get; set; }
        public string? FIODoctor { get; set; }
        public string? FIODepartmentHead { get; set; }
    }
}

