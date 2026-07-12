namespace Application.Sessions;

public enum SessionCommandStatus
{
    Succeeded = 1,
    NotFound = 2,
    Conflict = 3,
    Forbidden = 4
}