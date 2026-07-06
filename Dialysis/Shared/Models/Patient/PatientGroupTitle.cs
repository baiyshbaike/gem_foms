
using System;
namespace Dialysis.Shared.Models
{
	public class PatientGroupTitle
	{
        public long Id { get; set; }
        public string? SignFile { get; set; }
        public string? Description { get; set; }       
        public long? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }      
        
    }
}

