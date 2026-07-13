namespace Application.Admin;

public sealed record AdminUserCommandResult<T>(AdminUserCommandStatus Status, T? Value = default);
