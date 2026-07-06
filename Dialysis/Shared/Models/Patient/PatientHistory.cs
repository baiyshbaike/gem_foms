using System;
namespace Dialysis.Shared.Models
{
    public class PatientHistory
    {
        public long Id { get; set; }
        public long PatientId { get; set; }
        public long GroupId { get; set; }
        public long GroupTitleId { get; set; }
        public string? TextCause { get; set; } 
        public long? GroupFromId { get; set; }
        public string? GroupText { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ActDate { get; set; }
        public string? UniqID { get; set; }        
        public long? GroupReasonId { get; set; }
        public long? GroupLPUId { get; set; }
    }
}

