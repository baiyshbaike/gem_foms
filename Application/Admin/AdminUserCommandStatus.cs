namespace Application.Admin;

public enum AdminUserCommandStatus
{
    Succeeded = 1,
    NotFound = 2,
    Conflict = 3,
    ValidationFailed = 4
}
