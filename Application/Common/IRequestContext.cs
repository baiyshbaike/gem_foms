namespace Application.Common;

public interface IRequestContext
{
    long? UserId { get; }
    string? Username { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
    string? HttpMethod { get; }
    string? Path { get; }
    string CorrelationId { get; }
}