using System;
namespace Dialysis.Shared.Models
{
	public class UserRole
    {
        public long Id { get; set; }
        public long? UserId { get; set; }
        public long? RoleId { get; set; }
       
    }
}

