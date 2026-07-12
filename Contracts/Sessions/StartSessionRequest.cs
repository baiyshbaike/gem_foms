namespace Contracts.Sessions;

public sealed record StartSessionRequest(
    long MachineId,
    SessionMeasurementRequest? StartMeasurement);