using Contracts.Admin;

namespace Application.Admin;

public interface IAuditLogQueryService
{
    Task<IReadOnlyList<AuditLogDto>> GetLatestAsync(int take, CancellationToken cancellationToken);
}