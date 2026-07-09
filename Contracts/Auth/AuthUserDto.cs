namespace Contracts.Auth;

public sealed record AuthUserDto(long Id, string Username, string FirstName, string LastName, IReadOnlyList<string> Permissions);