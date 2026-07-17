using Application.Audit;
using Application.Sessions;
using Application.Tenants;
using Contracts.Sessions;
using Domain.MedCards;
using Domain.Patients;
using Domain.Sessions;
using Infrastructure.Common;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Domain.MedCenters;

namespace Infrastructure.Sessions;

public sealed class HdSessionService : IHdSessionService
{
    private readonly AppDbContext _db;
    private readonly IActionLogService _actionLogService;
    private readonly ITenantAccessService _tenantAccessService;
    private readonly RegionalSettings _regionalSettings;

    public HdSessionService(
        AppDbContext db,
        IActionLogService actionLogService,
        ITenantAccessService tenantAccessService,
        RegionalSettings regionalSettings)
    {
        _db = db;
        _actionLogService = actionLogService;
        _tenantAccessService = tenantAccessService;
        _regionalSettings = regionalSettings;
    }

    public async Task<IReadOnlyList<SessionDto>?> GetAsync(
        long userId,
        IReadOnlyList<string>? tenantIds,
        CancellationToken cancellationToken)
    {
        var filter = await _tenantAccessService.ResolveTenantFilterAsync(userId, tenantIds, cancellationToken);
        if (!filter.Succeeded)
        {
            await AddActionLogAsync(
                userId,
                "SessionsViewFailed",
                null,
                403,
                false,
                "Tenant filter contains forbidden tenant",
                null,
                cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return null;
        }

        var sessions = await _db.HdSessions
            .AsNoTracking()
            .Where(x => filter.TenantIds.Contains(x.TenantId))
            .OrderByDescending(x => x.Id)
            .Select(x => new SessionDto(
                x.Id,
                x.TenantId,
                x.PatientId,
                x.MedCardId,
                x.MachineId,
                x.Status.ToString(),
                x.IdentifiedAt,
                x.StartedAt,
                x.FinishedAt,
                x.EndIdentifiedAt,
                x.SentToPayAt,
                x.PaidAt,
                x.ActiveMinutes,
                x.PauseMinutes))
            .ToListAsync(cancellationToken);

        await AddActionLogAsync(
            userId,
            "SessionsViewed",
            null,
            200,
            true,
            null,
            $$"""{"tenantCount":{{filter.TenantIds.Count}},"resultCount":{{sessions.Count}}}""",
            cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return sessions;
    }

    public async Task<SessionDto?> GetByIdAsync(
        long userId,
        string tenantId,
        long id,
        CancellationToken cancellationToken)
    {
        if (!await _tenantAccessService.CanAccessTenantAsync(userId, tenantId, cancellationToken))
        {
            await AddActionLogAsync(
                userId,
                "SessionViewFailed",
                id.ToString(),
                403,
                false,
                "Tenant access denied",
                $$"""{"tenantId":"{{tenantId}}"}""",
                cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return null;
        }

        var session = await _db.HdSessions
            .AsNoTracking()
            .Where(x => x.Id == id && x.TenantId == tenantId)
            .Select(x => new SessionDto(
                x.Id,
                x.TenantId,
                x.PatientId,
                x.MedCardId,
                x.MachineId,
                x.Status.ToString(),
                x.IdentifiedAt,
                x.StartedAt,
                x.FinishedAt,
                x.EndIdentifiedAt,
                x.SentToPayAt,
                x.PaidAt,
                x.ActiveMinutes,
                x.PauseMinutes))
            .FirstOrDefaultAsync(cancellationToken);

        await AddActionLogAsync(
            userId,
            session is null ? "SessionViewFailed" : "SessionViewed",
            id.ToString(),
            session is null ? 404 : 200,
            session is not null,
            session is null ? "Session not found" : null,
            $$"""{"tenantId":"{{tenantId}}"}""",
            cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return session;
    }

    public async Task<SessionCommandResult<SessionDto>> CreateIdentifiedAsync(
        string tenantId,
        long userId,
        CreateSessionRequest request,
        CancellationToken cancellationToken)
    {
        if (!await _tenantAccessService.CanAccessTenantAsync(userId, tenantId, cancellationToken))
        {
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.Forbidden);
        }

        var medCard = await _db.MedCards
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.Id == request.MedCardId &&
                x.TenantId == tenantId &&
                !x.IsDeleted,
                cancellationToken);

        if (medCard is null)
        {
            await AddActionLogAsync(
                userId,
                "SessionCreateFailed",
                null,
                404,
                false,
                "MedCard not found in active tenant",
                $$"""{"tenantId":"{{tenantId}}","medCardId":{{request.MedCardId}}}""",
                cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.NotFound);
        }

        if (medCard.Status != MedCardStatus.Open)
        {
            await AddActionLogAsync(
                userId,
                "SessionCreateFailed",
                null,
                409,
                false,
                "MedCard is not open",
                $$"""{"tenantId":"{{tenantId}}","medCardId":{{medCard.Id}}}""",
                cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.Conflict);
        }

        var patientExists = await _db.Patients.AnyAsync(x =>
            x.Id == medCard.PatientId &&
            !x.IsDeleted &&
            x.IsActive &&
            x.GroupId != PatientGroupIds.Archive,
            cancellationToken);

        if (!patientExists)
        {
            await AddActionLogAsync(
                userId,
                "SessionCreateFailed",
                null,
                409,
                false,
                "Patient is inactive, deleted, or archived",
                $$"""{"tenantId":"{{tenantId}}","patientId":{{medCard.PatientId}}}""",
                cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.Conflict);
        }

        var patientHasActiveSession = await _db.HdSessions.AnyAsync(x =>
                x.PatientId == medCard.PatientId &&
                (
                    x.Status == SessionStatus.Identified ||
                    x.Status == SessionStatus.Started ||
                    x.Status == SessionStatus.Paused
                ),
            cancellationToken);

        if (patientHasActiveSession)
        {
            await AddActionLogAsync(
                userId,
                "SessionCreateFailed",
                null,
                409,
                false,
                "Patient already has active session",
                $$"""{"tenantId":"{{tenantId}}","patientId":{{medCard.PatientId}}}""",
                cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.Conflict);
        }

        var now = DateTimeOffset.UtcNow;
        var session = new HdSession
        {
            TenantId = tenantId,
            PatientId = medCard.PatientId,
            MedCardId = medCard.Id,
            Status = SessionStatus.Identified,
            IdentifiedAt = now,
            StatusChangedAt = now,
            StatusReason = "Created as identified"
        };

        _db.HdSessions.Add(session);
        await _db.SaveChangesAsync(cancellationToken);

        await AddActionLogAsync(
            userId,
            "SessionIdentified",
            session.Id.ToString(),
            201,
            true,
            null,
            $$"""{"tenantId":"{{tenantId}}","medCardId":{{session.MedCardId}},"patientId":{{session.PatientId}}}""",
            cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return new SessionCommandResult<SessionDto>(SessionCommandStatus.Succeeded, ToDto(session));
    }

    public async Task<SessionCommandResult<SessionDto>> StartAsync(
        string tenantId,
        long userId,
        long sessionId,
        StartSessionRequest request,
        bool hasOverride,
        CancellationToken cancellationToken)
    {
        if (!await _tenantAccessService.CanAccessTenantAsync(userId, tenantId, cancellationToken))
        {
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.Forbidden);
        }

        var session = await _db.HdSessions
            .FirstOrDefaultAsync(x => x.Id == sessionId && x.TenantId == tenantId, cancellationToken);

        if (session is null)
        {
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.NotFound);
        }

        if (session.Status != SessionStatus.Identified)
        {
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.Conflict);
        }

        var settings = await GetSettingsAsync(tenantId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var startDeadline = session.IdentifiedAt.AddMinutes(settings.IdentificationStartLimitMinutes);

        if (now > startDeadline && !hasOverride)
        {
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.Conflict);
        }

        var patientExists = await _db.Patients.AnyAsync(x =>
            x.Id == session.PatientId &&
            !x.IsDeleted &&
            x.IsActive &&
            x.GroupId != PatientGroupIds.Archive,
            cancellationToken);

        if (!patientExists)
        {
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.Conflict);
        }

        var medCardIsOpen = await _db.MedCards.AnyAsync(x =>
            x.Id == session.MedCardId &&
            x.PatientId == session.PatientId &&
            x.TenantId == tenantId &&
            !x.IsDeleted &&
            x.Status == MedCardStatus.Open,
            cancellationToken);

        if (!medCardIsOpen)
        {
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.Conflict);
        }

        var patientHasRunningSession = await _db.HdSessions.AnyAsync(x =>
            x.Id != session.Id &&
            x.PatientId == session.PatientId &&
            (x.Status == SessionStatus.Started || x.Status == SessionStatus.Paused),
            cancellationToken);

        if (patientHasRunningSession)
        {
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.Conflict);
        }

        var machineHasRunningSession = await _db.HdSessions.AnyAsync(x =>
            x.Id != session.Id &&
            x.TenantId == tenantId &&
            x.MachineId == request.MachineId &&
            (x.Status == SessionStatus.Started || x.Status == SessionStatus.Paused),
            cancellationToken);

        if (machineHasRunningSession)
        {
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.Conflict);
        }
        var machine = await _db.MedCenterMachines
            .AsNoTracking()
            .Where(x =>
                x.Id == request.MachineId &&
                x.TenantId == tenantId &&
                !x.IsDeleted)
            .Select(x => new
            {
                x.Id,
                x.IsActive,
                x.IsApproved,
                x.DailySessionLimit
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (machine is null)
        {
            await AddActionLogAsync(
                userId,
                "SessionStartFailed",
                session.Id.ToString(),
                404,
                false,
                "Machine not found in active tenant",
                $$"""{"tenantId":"{{tenantId}}","machineId":{{request.MachineId}}}""",
                cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.NotFound);
        }

        if (!machine.IsActive || !machine.IsApproved)
        {
            await AddActionLogAsync(
                userId,
                "SessionStartFailed",
                session.Id.ToString(),
                409,
                false,
                "Machine is inactive or not approved",
                $$"""{"tenantId":"{{tenantId}}","machineId":{{request.MachineId}}}""",
                cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.Conflict);
        }

        var dayRange = _regionalSettings.GetUtcDayRange(now);

        var machineSessionCountToday = await _db.HdSessions.CountAsync(x =>
                x.TenantId == tenantId &&
                x.MachineId == request.MachineId &&
                x.StartedAt >= dayRange.Start &&
                x.StartedAt < dayRange.End &&
                x.Status != SessionStatus.Cancelled,
            cancellationToken);

        if (machineSessionCountToday >= machine.DailySessionLimit)
        {
            await AddActionLogAsync(
                userId,
                "SessionStartFailed",
                session.Id.ToString(),
                409,
                false,
                "Machine daily session limit reached",
                $$"""{"tenantId":"{{tenantId}}","machineId":{{request.MachineId}},"dailySessionLimit":{{machine.DailySessionLimit}}}""",
                cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.Conflict);
        }
        session.MachineId = request.MachineId;
        session.Status = SessionStatus.Started;
        session.StartedAt ??= now;
        session.StatusChangedAt = now;
        session.StatusReason = "Started by doctor";

        if (request.StartMeasurement is not null)
        {
            await AddOrUpdateMeasurementEntityAsync(
                session,
                SessionMeasurementPoint.Start,
                request.StartMeasurement,
                now,
                cancellationToken);
        }

        await AddActionLogAsync(
            userId,
            "SessionStarted",
            session.Id.ToString(),
            200,
            true,
            null,
            $$"""{"tenantId":"{{tenantId}}","machineId":{{request.MachineId}}}""",
            cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return new SessionCommandResult<SessionDto>(SessionCommandStatus.Succeeded, ToDto(session));
    }

    public async Task<SessionCommandResult<SessionDto>> PauseAsync(
        string tenantId,
        long userId,
        long sessionId,
        PauseSessionRequest request,
        CancellationToken cancellationToken)
    {
        if (!await _tenantAccessService.CanAccessTenantAsync(userId, tenantId, cancellationToken))
        {
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.Forbidden);
        }

        var session = await _db.HdSessions
            .FirstOrDefaultAsync(x => x.Id == sessionId && x.TenantId == tenantId, cancellationToken);

        if (session is null)
        {
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.NotFound);
        }

        if (session.Status != SessionStatus.Started)
        {
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.Conflict);
        }

        var hasOpenPause = await _db.HdSessionPauses.AnyAsync(x =>
            x.HdSessionId == session.Id &&
            x.EndedAt == null,
            cancellationToken);

        if (hasOpenPause)
        {
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.Conflict);
        }

        var now = DateTimeOffset.UtcNow;
        _db.HdSessionPauses.Add(new HdSessionPause
        {
            TenantId = tenantId,
            HdSessionId = session.Id,
            StartedAt = now,
            Reason = request.Reason,
            PausedBy = userId
        });

        session.Status = SessionStatus.Paused;
        session.StatusChangedAt = now;
        session.StatusReason = "Paused by doctor";

        await AddActionLogAsync(
            userId,
            "SessionPaused",
            session.Id.ToString(),
            200,
            true,
            null,
            $$"""{"tenantId":"{{tenantId}}"}""",
            cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return new SessionCommandResult<SessionDto>(SessionCommandStatus.Succeeded, ToDto(session));
    }

    public async Task<SessionCommandResult<SessionDto>> ResumeAsync(
        string tenantId,
        long userId,
        long sessionId,
        CancellationToken cancellationToken)
    {
        if (!await _tenantAccessService.CanAccessTenantAsync(userId, tenantId, cancellationToken))
        {
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.Forbidden);
        }

        var session = await _db.HdSessions
            .FirstOrDefaultAsync(x => x.Id == sessionId && x.TenantId == tenantId, cancellationToken);

        if (session is null)
        {
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.NotFound);
        }

        if (session.Status != SessionStatus.Paused)
        {
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.Conflict);
        }

        var pause = await _db.HdSessionPauses
            .FirstOrDefaultAsync(x =>
                x.HdSessionId == session.Id &&
                x.EndedAt == null,
                cancellationToken);

        if (pause is null)
        {
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.Conflict);
        }

        var now = DateTimeOffset.UtcNow;
        pause.EndedAt = now;
        pause.ResumedBy = userId;

        session.Status = SessionStatus.Started;
        session.StatusChangedAt = now;
        session.StatusReason = "Resumed by doctor";

        await AddActionLogAsync(
            userId,
            "SessionResumed",
            session.Id.ToString(),
            200,
            true,
            null,
            $$"""{"tenantId":"{{tenantId}}","pauseId":{{pause.Id}}}""",
            cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return new SessionCommandResult<SessionDto>(SessionCommandStatus.Succeeded, ToDto(session));
    }

    public async Task<SessionCommandResult<SessionDto>> FinishAsync(
        string tenantId,
        long userId,
        long sessionId,
        FinishSessionRequest request,
        CancellationToken cancellationToken)
    {
        if (!await _tenantAccessService.CanAccessTenantAsync(userId, tenantId, cancellationToken))
        {
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.Forbidden);
        }

        var session = await _db.HdSessions
            .Include(x => x.Pauses)
            .FirstOrDefaultAsync(x => x.Id == sessionId && x.TenantId == tenantId, cancellationToken);

        if (session is null)
        {
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.NotFound);
        }

        if (session.Status != SessionStatus.Started || session.StartedAt is null)
        {
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.Conflict);
        }

        if (session.Pauses.Any(x => x.EndedAt == null))
        {
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.Conflict);
        }

        var now = DateTimeOffset.UtcNow;
        var activeDuration = SessionDurationCalculator.CalculateActiveDuration(
            session.StartedAt.Value,
            now,
            session.Pauses.ToList());
        var pauseDuration = SessionDurationCalculator.CalculatePauseDuration(now, session.Pauses.ToList());

        session.Status = SessionStatus.Finished;
        session.FinishedAt = now;
        session.StatusChangedAt = now;
        session.StatusReason = "Finished by doctor";
        session.ActiveMinutes = (int)Math.Round(activeDuration.TotalMinutes);
        session.PauseMinutes = (int)Math.Round(pauseDuration.TotalMinutes);

        if (request.EndMeasurement is not null)
        {
            await AddOrUpdateMeasurementEntityAsync(
                session,
                SessionMeasurementPoint.End,
                request.EndMeasurement,
                now,
                cancellationToken);
        }

        await AddActionLogAsync(
            userId,
            "SessionFinished",
            session.Id.ToString(),
            200,
            true,
            null,
            $$"""{"tenantId":"{{tenantId}}","activeMinutes":{{session.ActiveMinutes}},"pauseMinutes":{{session.PauseMinutes}}}""",
            cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return new SessionCommandResult<SessionDto>(SessionCommandStatus.Succeeded, ToDto(session));
    }

    public async Task<SessionCommandResult<SessionDto>> EndIdentifyAsync(
        string tenantId,
        long userId,
        long sessionId,
        bool hasOverride,
        CancellationToken cancellationToken)
    {
        if (!await _tenantAccessService.CanAccessTenantAsync(userId, tenantId, cancellationToken))
        {
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.Forbidden);
        }

        var session = await _db.HdSessions
            .FirstOrDefaultAsync(x => x.Id == sessionId && x.TenantId == tenantId, cancellationToken);

        if (session is null)
        {
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.NotFound);
        }

        if (session.Status == SessionStatus.Finished)
        {
            if (session.FinishedAt is null)
            {
                return new SessionCommandResult<SessionDto>(SessionCommandStatus.Conflict);
            }

            var settings = await GetSettingsAsync(tenantId, cancellationToken);
            var deadline = session.FinishedAt.Value.AddMinutes(settings.EndIdentificationLimitMinutes);

            if (DateTimeOffset.UtcNow > deadline && !hasOverride)
            {
                return new SessionCommandResult<SessionDto>(SessionCommandStatus.Conflict);
            }
        }

        if (!SessionTransitionService.CanMove(session.Status, SessionStatus.EndIdentified, hasOverride))
        {
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.Conflict);
        }

        var now = DateTimeOffset.UtcNow;
        session.Status = SessionStatus.EndIdentified;
        session.EndIdentifiedAt = now;
        session.StatusChangedAt = now;
        session.StatusReason = hasOverride ? "End identified by override" : "End identified";

        await AddActionLogAsync(
            userId,
            "SessionEndIdentified",
            session.Id.ToString(),
            200,
            true,
            null,
            $$"""{"tenantId":"{{tenantId}}","override":{{hasOverride.ToString().ToLowerInvariant()}}}""",
            cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return new SessionCommandResult<SessionDto>(SessionCommandStatus.Succeeded, ToDto(session));
    }

    public async Task<SessionCommandResult<SessionDto>> SendToPayAsync(
        string tenantId,
        long userId,
        long sessionId,
        bool hasOverride,
        CancellationToken cancellationToken)
    {
        if (!await _tenantAccessService.CanAccessTenantAsync(userId, tenantId, cancellationToken))
        {
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.Forbidden);
        }

        var session = await _db.HdSessions
            .FirstOrDefaultAsync(x => x.Id == sessionId && x.TenantId == tenantId, cancellationToken);

        if (session is null)
        {
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.NotFound);
        }

        if (session.Status == SessionStatus.EndIdentified)
        {
            if (session.EndIdentifiedAt is null)
            {
                return new SessionCommandResult<SessionDto>(SessionCommandStatus.Conflict);
            }

            var settings = await GetSettingsAsync(tenantId, cancellationToken);
            var deadline = session.EndIdentifiedAt.Value.AddMinutes(settings.SendToPayLimitMinutes);

            if (DateTimeOffset.UtcNow > deadline && !hasOverride)
            {
                return new SessionCommandResult<SessionDto>(SessionCommandStatus.Conflict);
            }
        }

        if (!SessionTransitionService.CanMove(session.Status, SessionStatus.SentToPay, hasOverride))
        {
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.Conflict);
        }

        var now = DateTimeOffset.UtcNow;
        session.Status = SessionStatus.SentToPay;
        session.SentToPayAt = now;
        session.StatusChangedAt = now;
        session.StatusReason = hasOverride ? "Sent to pay by override" : "Sent to pay";

        await AddActionLogAsync(
            userId,
            "SessionSentToPay",
            session.Id.ToString(),
            200,
            true,
            null,
            $$"""{"tenantId":"{{tenantId}}","override":{{hasOverride.ToString().ToLowerInvariant()}}}""",
            cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return new SessionCommandResult<SessionDto>(SessionCommandStatus.Succeeded, ToDto(session));
    }

    public async Task<SessionCommandResult<SessionDto>> MarkPaidAsync(
        string tenantId,
        long userId,
        long sessionId,
        CancellationToken cancellationToken)
    {
        if (!await _tenantAccessService.CanAccessTenantAsync(userId, tenantId, cancellationToken))
        {
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.Forbidden);
        }

        var session = await _db.HdSessions
            .FirstOrDefaultAsync(x => x.Id == sessionId && x.TenantId == tenantId, cancellationToken);

        if (session is null)
        {
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.NotFound);
        }

        if (!SessionTransitionService.CanMove(session.Status, SessionStatus.Paid))
        {
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.Conflict);
        }

        var now = DateTimeOffset.UtcNow;
        session.Status = SessionStatus.Paid;
        session.PaidAt = now;
        session.StatusChangedAt = now;
        session.StatusReason = "Paid";

        await AddActionLogAsync(
            userId,
            "SessionPaid",
            session.Id.ToString(),
            200,
            true,
            null,
            $$"""{"tenantId":"{{tenantId}}"}""",
            cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return new SessionCommandResult<SessionDto>(SessionCommandStatus.Succeeded, ToDto(session));
    }

    public async Task<SessionCommandResult<SessionDto>> ArchiveAsync(
        string tenantId,
        long userId,
        long sessionId,
        bool hasOverride,
        CancellationToken cancellationToken)
    {
        if (!await _tenantAccessService.CanAccessTenantAsync(userId, tenantId, cancellationToken))
        {
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.Forbidden);
        }

        var session = await _db.HdSessions
            .FirstOrDefaultAsync(x => x.Id == sessionId && x.TenantId == tenantId, cancellationToken);

        if (session is null)
        {
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.NotFound);
        }

        if (!SessionTransitionService.CanMove(session.Status, SessionStatus.Archived, hasOverride))
        {
            return new SessionCommandResult<SessionDto>(SessionCommandStatus.Conflict);
        }

        var now = DateTimeOffset.UtcNow;
        session.Status = SessionStatus.Archived;
        session.ArchivedAt = now;
        session.StatusChangedAt = now;
        session.StatusReason = hasOverride ? "Archived by override" : "Archived";

        await AddActionLogAsync(
            userId,
            "SessionArchived",
            session.Id.ToString(),
            200,
            true,
            null,
            $$"""{"tenantId":"{{tenantId}}","override":{{hasOverride.ToString().ToLowerInvariant()}}}""",
            cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return new SessionCommandResult<SessionDto>(SessionCommandStatus.Succeeded, ToDto(session));
    }

    public async Task<SessionCommandResult<SessionMeasurementDto>> AddOrUpdateMeasurementAsync(
        string tenantId,
        long userId,
        long sessionId,
        SessionMeasurementPoint point,
        SessionMeasurementRequest request,
        CancellationToken cancellationToken)
    {
        if (!await _tenantAccessService.CanAccessTenantAsync(userId, tenantId, cancellationToken))
        {
            return new SessionCommandResult<SessionMeasurementDto>(SessionCommandStatus.Forbidden);
        }

        var session = await _db.HdSessions
            .FirstOrDefaultAsync(x => x.Id == sessionId && x.TenantId == tenantId, cancellationToken);

        if (session is null)
        {
            return new SessionCommandResult<SessionMeasurementDto>(SessionCommandStatus.NotFound);
        }

        var measurement = await AddOrUpdateMeasurementEntityAsync(
            session,
            point,
            request,
            DateTimeOffset.UtcNow,
            cancellationToken);

        await AddActionLogAsync(
            userId,
            "SessionMeasurementUpdated",
            session.Id.ToString(),
            200,
            true,
            null,
            $$"""{"tenantId":"{{tenantId}}","point":"{{point}}"}""",
            cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return new SessionCommandResult<SessionMeasurementDto>(
            SessionCommandStatus.Succeeded,
            ToMeasurementDto(measurement));
    }

    private async Task<SessionWorkflowSettings> GetSettingsAsync(
        string tenantId,
        CancellationToken cancellationToken)
    {
        var settings = await _db.SessionWorkflowSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings ?? new SessionWorkflowSettings
        {
            TenantId = tenantId,
            IdentificationStartLimitMinutes = 240,
            AutoFinishActiveMinutes = 270,
            EndIdentificationLimitMinutes = 120,
            SendToPayLimitMinutes = 360
        };
    }

    private async Task<HdSessionMeasurement> AddOrUpdateMeasurementEntityAsync(
        HdSession session,
        SessionMeasurementPoint point,
        SessionMeasurementRequest request,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var measurement = await _db.HdSessionMeasurements
            .FirstOrDefaultAsync(x =>
                x.HdSessionId == session.Id &&
                x.Point == point,
                cancellationToken);

        if (measurement is null)
        {
            measurement = new HdSessionMeasurement
            {
                TenantId = session.TenantId,
                HdSessionId = session.Id,
                Point = point
            };

            _db.HdSessionMeasurements.Add(measurement);
        }

        measurement.Sys = request.Sys;
        measurement.Dia = request.Dia;
        measurement.Temp = request.Temp;
        measurement.Ritm = request.Ritm;
        measurement.MeasuredAt = request.MeasuredAt ?? now;
        measurement.Note = request.Note;

        return measurement;
    }

    private async Task AddActionLogAsync(
        long userId,
        string action,
        string? entityId,
        int statusCode,
        bool succeeded,
        string? failureReason,
        string? metadataJson,
        CancellationToken cancellationToken)
    {
        await _actionLogService.AddAsync(new ActionLogRequest
        {
            UserId = userId,
            Action = action,
            Module = "session",
            EntityName = "HdSession",
            EntityId = entityId,
            StatusCode = statusCode,
            Succeeded = succeeded,
            FailureReason = failureReason,
            MetadataJson = metadataJson
        }, cancellationToken);
    }

    private static SessionDto ToDto(HdSession session)
    {
        return new SessionDto(
            session.Id,
            session.TenantId,
            session.PatientId,
            session.MedCardId,
            session.MachineId,
            session.Status.ToString(),
            session.IdentifiedAt,
            session.StartedAt,
            session.FinishedAt,
            session.EndIdentifiedAt,
            session.SentToPayAt,
            session.PaidAt,
            session.ActiveMinutes,
            session.PauseMinutes);
    }

    private static SessionMeasurementDto ToMeasurementDto(HdSessionMeasurement measurement)
    {
        return new SessionMeasurementDto(
            measurement.Id,
            measurement.Point.ToString(),
            measurement.Sys,
            measurement.Dia,
            measurement.Temp,
            measurement.Ritm,
            measurement.MeasuredAt,
            measurement.Note);
    }
}
