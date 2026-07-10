using Application.Admin;
using Contracts.Admin;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Admin;

public sealed class AuditLogQueryService : IAuditLogQueryService
{
    private readonly AppDbContext _db;

    public AuditLogQueryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetLatestAsync(int take, CancellationToken cancellationToken)
    {
        return await _db.ActionLogs
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
    }
}