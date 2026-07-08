namespace Dialysis.Domain.Users;

public sealed class Role
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsSystem { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}