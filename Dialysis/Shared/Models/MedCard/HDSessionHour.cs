using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dialysis.Shared.Models
{
	public class HDSessionHour
	{
        public long Id { get; set; }       
        public long HDSessionId { get; set; }
        public string? Title { get; set; }
        public string? Hour { get; set; }
        public string? Sys { get; set; }
        public string? Dia{ get; set; }
        public string? Temp { get; set; }
        public string? Ritm { get; set; }       
        public long? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public long? LastModifiedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }

    }
}

