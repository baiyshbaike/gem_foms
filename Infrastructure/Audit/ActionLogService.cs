using Application.Audit;
using Application.Common;
using Domain.Audit;
using Infrastructure.Data;

namespace Infrastructure.Audit;

public sealed class ActionLogService : IActionLogService
{
    private readonly AppDbContext _db;
    private readonly IRequestContext  _requestContext;

    public ActionLogService(AppDbContext db, IRequestContext requestContext)
    {
        _db = db;
        _requestContext = requestContext;
    }
    
    public Task AddAsync(ActionLogRequest request, CancellationToken cancellationToken)
    {
        _db.ActionLogs.Add(new ActionLog
        {
            UserId = request.UserId ?? _requestContext.UserId,
            UsernameSnapshot = request.UsernameSnapshot ?? _requestContext.Username,
            Action = request.Action,
            Module = request.Module,
            EntityName = request.EntityName,
            EntityId = request.EntityId,
            HttpMethod = _requestContext.HttpMethod,
            Path = _requestContext.Path,
            IpAddress = _requestContext.IpAddress,
            UserAgent = _requestContext.UserAgent,
            StatusCode = request.StatusCode,
            Succeeded = request.Succeeded,
            FailureReason = request.FailureReason,
            CorrelationId = _requestContext.CorrelationId,
            MetadataJson = request.MetadataJson,
            CreatedAt = DateTimeOffset.UtcNow
        });
        return Task.CompletedTask;
    }
}