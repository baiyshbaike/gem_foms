using Contracts.Sessions;
using Domain.Sessions;

namespace Application.Sessions;

public interface IHdSessionService
{
    Task<IReadOnlyList<SessionDto>?> GetAsync(
        long userId,
        IReadOnlyList<string>? tenantIds,
        CancellationToken cancellationToken);

    Task<SessionDto?> GetByIdAsync(
        long userId,
        string tenantId,
        long id,
        CancellationToken cancellationToken);

    Task<SessionCommandResult<SessionDto>> CreateIdentifiedAsync(
        string tenantId,
        long userId,
        CreateSessionRequest request,
        CancellationToken cancellationToken);

    Task<SessionCommandResult<SessionDto>> StartAsync(
        string tenantId,
        long userId,
        long sessionId,
        StartSessionRequest request,
        bool hasOverride,
        CancellationToken cancellationToken);

    Task<SessionCommandResult<SessionDto>> PauseAsync(
        string tenantId,
        long userId,
        long sessionId,
        PauseSessionRequest request,
        CancellationToken cancellationToken);

    Task<SessionCommandResult<SessionDto>> ResumeAsync(
        string tenantId,
        long userId,
        long sessionId,
        CancellationToken cancellationToken);

    Task<SessionCommandResult<SessionDto>> FinishAsync(
        string tenantId,
        long userId,
        long sessionId,
        FinishSessionRequest request,
        CancellationToken cancellationToken);

    Task<SessionCommandResult<SessionDto>> EndIdentifyAsync(
        string tenantId,
        long userId,
        long sessionId,
        bool hasOverride,
        CancellationToken cancellationToken);

    Task<SessionCommandResult<SessionDto>> SendToPayAsync(
        string tenantId,
        long userId,
        long sessionId,
        bool hasOverride,
        CancellationToken cancellationToken);

    Task<SessionCommandResult<SessionDto>> MarkPaidAsync(
        string tenantId,
        long userId,
        long sessionId,
        CancellationToken cancellationToken);

    Task<SessionCommandResult<SessionDto>> ArchiveAsync(
        string tenantId,
        long userId,
        long sessionId,
        bool hasOverride,
        CancellationToken cancellationToken);

    Task<SessionCommandResult<SessionMeasurementDto>> AddOrUpdateMeasurementAsync(
        string tenantId,
        long userId,
        long sessionId,
        SessionMeasurementPoint point,
        SessionMeasurementRequest request,
        CancellationToken cancellationToken);
}