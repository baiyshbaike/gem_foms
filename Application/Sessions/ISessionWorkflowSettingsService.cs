using Contracts.Sessions;

namespace Application.Sessions;

public interface ISessionWorkflowSettingsService
{
    Task<SessionWorkflowSettingsDto> GetAsync(string tenantId, CancellationToken cancellationToken);

    Task<SessionWorkflowSettingsDto> UpdateAsync(
        string tenantId,
        long userId,
        UpdateSessionWorkflowSettingsRequest request,
        CancellationToken cancellationToken);
}
