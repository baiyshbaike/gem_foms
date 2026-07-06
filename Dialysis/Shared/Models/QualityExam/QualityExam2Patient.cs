using System;
namespace Dialysis.Shared.Models
{
	public class QualityExam2Patient
    {
        public long Id { get; set; }
        public long ExamId { get; set; }
        public string? MedCardNo { get; set; }
        public string? PatientPin { get; set; }
        public DateTime? SessionDate { get; set; }
       
    }
}

