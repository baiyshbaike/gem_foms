using System;
namespace Dialysis.Shared.Models
{
	public class MedCenterUser
	{
        public long Id { get; set; }
        public long MedCenterId { get; set; }
        public long UserId { get; set; }
    }
}

