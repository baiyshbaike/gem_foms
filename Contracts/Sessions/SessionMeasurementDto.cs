namespace Contracts.Sessions;

public sealed record SessionMeasurementDto(
    long Id,
    string Point,
    string? Sys,
    string? Dia,
    string? Temp,
    string? Ritm,
    DateTimeOffset? MeasuredAt,
    string? Note);