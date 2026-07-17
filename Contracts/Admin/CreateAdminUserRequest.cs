using System.ComponentModel.DataAnnotations;

namespace Contracts.Admin;

public sealed class CreateAdminUserRequest
{
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    [MaxLength(200)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    [MinLength(1)]
    public IReadOnlyList<long> RoleIds { get; set; } = [];

    [Range(1, long.MaxValue)]
    public long? ManagerRegionId { get; set; }
}
