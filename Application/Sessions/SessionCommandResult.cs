namespace Application.Sessions;

public sealed record SessionCommandResult<T>(
    SessionCommandStatus Status,
    T? Value = default);