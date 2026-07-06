using System;
using Dialysis.Shared.Models;
using Dialysis.Shared.Models.Files;

namespace Dialysis.Shared.Dto
{
	public class UserDto
	{
		public bool ShowDetails { get; set; } = false;
        public User User { get; set; }
        public UserProfile UserProfile { get; set; }
        public List<Role> UserRoles { get; set; }
        public List<MedCenter> MedCenters { get; set; }
        public List<SaveFile> SaveFiles { get; set; }
    }
}

