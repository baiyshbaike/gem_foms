using System;
namespace Dialysis.Shared.Models
{
    public class User
    {
        public long Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MIddleName { get; set; }
        public string Username { get; set; }
        public string? Email { get; set; }

        public string Password { get; set; }        
        public long? CreatedBy { get; set; }
        public long ProfileId { get; set; }        
        public DateTime CreatedOn { get; set; }
        public long? LastModifiedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public DateTime? LastLogin { get; set; }
        public string? LastIp { get; set; }
        public bool IsActive { get; set; }
        public int AccessFailedCount { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public int TokenVersion { get; set; }
        public DateTime? LastLogoutDate { get; set; }
    }
}

