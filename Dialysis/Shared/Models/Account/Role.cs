using System;
namespace Dialysis.Shared.Models
{
	public class Role
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}

