using Application.Admin;
using Application.Audit;
using Contracts.Admin;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Admin;

public sealed class AuditLogQueryService : IAuditLogQueryService
{
    private readonly AppDbContext _db;
    private readonly IActionLogService _actionLogService;

    public AuditLogQueryService(AppDbContext db, IActionLogService actionLogService)
    {
        _db = db;
        _actionLogService = actionLogService;
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetLatestAsync(int take, CancellationToken cancellationToken)
    {
        var logs = await _db.ActionLogs
            .AsNoTracking()
            .OrderByDescending(x => x.Id)
            .Take(take)
            .Select(x => new AuditLogDto(
                x.Id,
                x.UserId,
                x.UsernameSnapshot,
                x.Action,
                x.Module,
                x.Succeeded,
                x.FailureReason,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        await _actionLogService.AddAsync(new ActionLogRequest
        {
            Action = "AuditLogsViewed",
            Module = "admin",
            EntityName = "ActionLog",
            StatusCode = 200,
            Succeeded = true,
            MetadataJson = $$"""{"take":{{take}},"resultCount":{{logs.Count}}}"""
        }, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return logs;
    }
}
