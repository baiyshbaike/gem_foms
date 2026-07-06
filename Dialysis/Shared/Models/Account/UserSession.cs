using System;

namespace Dialysis.Shared.Models
{
    public class UserSession
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Jti { get; set; } = string.Empty;
        public int TokenVersion { get; set; }
        public DateTime TokenExpiry { get; set; }
        public string? IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
