using Domain.Sessions;
using Application.Audit;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Sessions;

public sealed class HdSessionWorkflowWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<HdSessionWorkflowWorker> _logger;

    public HdSessionWorkflowWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<HdSessionWorkflowWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HdSession workflow worker failed");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task ProcessAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var actionLogService = scope.ServiceProvider.GetRequiredService<IActionLogService>();
        var now = DateTimeOffset.UtcNow;

        var settingsByTenant = await db.SessionWorkflowSettings
            .AsNoTracking()
            .ToDictionaryAsync(x => x.TenantId, cancellationToken);

        await ExpireIdentificationsAsync(db, actionLogService, settingsByTenant, now, cancellationToken);
        await AutoFinishStartedSessionsAsync(db, actionLogService, settingsByTenant, now, cancellationToken);
        await MarkEndIdentificationOverdueAsync(db, actionLogService, settingsByTenant, now, cancellationToken);
        await MarkSendToPayOverdueAsync(db, actionLogService, settingsByTenant, now, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
    }

    private static SessionWorkflowSettings GetSettings(
        IReadOnlyDictionary<string, SessionWorkflowSettings> settingsByTenant,
        string tenantId)
    {
        return settingsByTenant.TryGetValue(tenantId, out var settings)
            ? settings
            : new SessionWorkflowSettings
            {
                TenantId = tenantId,
                IdentificationStartLimitMinutes = 240,
                AutoFinishActiveMinutes = 270,
                EndIdentificationLimitMinutes = 120,
                SendToPayLimitMinutes = 360
            };
    }

    private static async Task ExpireIdentificationsAsync(
        AppDbContext db,
        IActionLogService actionLogService,
        IReadOnlyDictionary<string, SessionWorkflowSettings> settingsByTenant,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var sessions = await db.HdSessions
            .Where(x => x.Status == SessionStatus.Identified)
            .ToListAsync(cancellationToken);

        foreach (var session in sessions)
        {
            var settings = GetSettings(settingsByTenant, session.TenantId);
            var deadline = session.IdentifiedAt.AddMinutes(settings.IdentificationStartLimitMinutes);

            if (now < deadline)
            {
                continue;
            }

            session.Status = SessionStatus.IdentificationExpired;
            session.StatusChangedAt = now;
            session.StatusReason = "Identification start time limit expired";

            await AddSystemActionLogAsync(
                actionLogService,
                "SessionIdentificationExpired",
                session,
                "Identification start time limit expired",
                cancellationToken);
        }
    }

    private static async Task AutoFinishStartedSessionsAsync(
        AppDbContext db,
        IActionLogService actionLogService,
        IReadOnlyDictionary<string, SessionWorkflowSettings> settingsByTenant,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var sessions = await db.HdSessions
            .Include(x => x.Pauses)
            .Where(x => x.Status == SessionStatus.Started && x.StartedAt != null)
            .ToListAsync(cancellationToken);

        foreach (var session in sessions)
        {
            var settings = GetSettings(settingsByTenant, session.TenantId);
            var activeDuration = SessionDurationCalculator.CalculateActiveDuration(
                session.StartedAt!.Value,
                now,
                session.Pauses.ToList());

            if (activeDuration.TotalMinutes < settings.AutoFinishActiveMinutes)
            {
                continue;
            }

            var pauseDuration = SessionDurationCalculator.CalculatePauseDuration(now, session.Pauses.ToList());

            session.Status = SessionStatus.Finished;
            session.FinishedAt = now;
            session.StatusChangedAt = now;
            session.ActiveMinutes = (int)Math.Round(activeDuration.TotalMinutes);
            session.PauseMinutes = (int)Math.Round(pauseDuration.TotalMinutes);
            session.StatusReason = "Auto finished by active duration limit";

            await AddSystemActionLogAsync(
                actionLogService,
                "SessionAutoFinished",
                session,
                "Auto finished by active duration limit",
                cancellationToken);
        }
    }

    private static async Task MarkEndIdentificationOverdueAsync(
        AppDbContext db,
        IActionLogService actionLogService,
        IReadOnlyDictionary<string, SessionWorkflowSettings> settingsByTenant,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var sessions = await db.HdSessions
            .Where(x => x.Status == SessionStatus.Finished && x.FinishedAt != null)
            .ToListAsync(cancellationToken);

        foreach (var session in sessions)
        {
            var settings = GetSettings(settingsByTenant, session.TenantId);
            var deadline = session.FinishedAt!.Value.AddMinutes(settings.EndIdentificationLimitMinutes);

            if (now < deadline)
            {
                continue;
            }

            session.Status = SessionStatus.EndIdentificationOverdue;
            session.StatusChangedAt = now;
            session.StatusReason = "End identification time limit expired";

            await AddSystemActionLogAsync(
                actionLogService,
                "SessionEndIdentificationOverdue",
                session,
                "End identification time limit expired",
                cancellationToken);
        }
    }

    private static async Task MarkSendToPayOverdueAsync(
        AppDbContext db,
        IActionLogService actionLogService,
        IReadOnlyDictionary<string, SessionWorkflowSettings> settingsByTenant,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var sessions = await db.HdSessions
            .Where(x => x.Status == SessionStatus.EndIdentified && x.EndIdentifiedAt != null)
            .ToListAsync(cancellationToken);

        foreach (var session in sessions)
        {
            var settings = GetSettings(settingsByTenant, session.TenantId);
            var deadline = session.EndIdentifiedAt!.Value.AddMinutes(settings.SendToPayLimitMinutes);

            if (now < deadline)
            {
                continue;
            }

            session.Status = SessionStatus.SendToPayOverdue;
            session.StatusChangedAt = now;
            session.StatusReason = "Send to pay time limit expired";

            await AddSystemActionLogAsync(
                actionLogService,
                "SessionSendToPayOverdue",
                session,
                "Send to pay time limit expired",
                cancellationToken);
        }
    }

    private static async Task AddSystemActionLogAsync(
        IActionLogService actionLogService,
        string action,
        HdSession session,
        string reason,
        CancellationToken cancellationToken)
    {
        await actionLogService.AddAsync(new ActionLogRequest
        {
            UserId = 0,
            UsernameSnapshot = "system",
            Action = action,
            Module = "session",
            EntityName = "HdSession",
            EntityId = session.Id.ToString(),
            StatusCode = 200,
            Succeeded = true,
            MetadataJson = $$"""{"tenantId":"{{session.TenantId}}","status":"{{session.Status}}","reason":"{{reason}}"}"""
        }, cancellationToken);
    }
}
