using System;
namespace Dialysis.Shared.Models
{
	public class HDSessionPause
	{
        public long Id { get; set; }
        public long HDSessionId { get; set; }       
        public string? Note { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }       
        public DateTime? PauseStart { get; set; }
        public DateTime? PauseEnd { get; set; }
        public double? TotalMinutes { get; set; }

    }
}

