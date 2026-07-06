
using System;
namespace Dialysis.Shared.Models
{
	public class PatientGroupPerson
	{
        public long Id { get; set; }
        public long GroupTitleId { get; set; }     
        public long? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }       
        public long PersonTitleId { get; set; }
        public string? PersonFio { get; set; }        
    }
}

