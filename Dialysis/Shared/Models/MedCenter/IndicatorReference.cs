using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dialysis.Shared.Models
{
	public class IndicatorReference
	{
        public long Id { get; set; }
        public string? Title { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}

