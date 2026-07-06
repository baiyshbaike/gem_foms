using System;
namespace Dialysis.Shared.Models
{
	public class QualityExam3Row
	{
        public long Id { get; set; }
        public long ExamId { get; set; }
        public string? Organisation { get; set; }
        public long? TotalMedCard { get; set; }
        public long? TotalSession { get; set; }        
        public long? TotalDefects { get; set; }
        public long? TotalNotDefects { get; set; }
        public long? Defects { get; set; }
        public long? DefectsTotal { get; set; }
        public long? Defects1 { get; set; }
        public long? Defects2 { get; set; }
        public long? Defects3 { get; set; }
        public long? DefectTreatment { get; set; }
        public long? DefectTreatment1 { get; set; }
        public long? DefectTreatment2 { get; set; }
        public string? Extra1 { get; set; }
        public string? Extra2 { get; set; }
        public string? Extra3 { get; set; }
       
    }
}

