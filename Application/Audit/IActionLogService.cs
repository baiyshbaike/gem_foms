namespace Application.Audit;

public interface IActionLogService
{
    Task AddAsync(ActionLogRequest request,CancellationToken cancellationToken);
}