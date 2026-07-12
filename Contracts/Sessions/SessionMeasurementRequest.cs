namespace Contracts.Sessions;

public sealed record SessionMeasurementRequest(
    string? Sys,
    string? Dia,
    string? Temp,
    string? Ritm,
    DateTimeOffset? MeasuredAt,
    string? Note);