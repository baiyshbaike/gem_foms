namespace Contracts.Sessions;

public sealed record SessionWorkflowSettingsDto(
    long Id,
    string TenantId,
    int IdentificationStartLimitMinutes,
    int AutoFinishActiveMinutes,
    int EndIdentificationLimitMinutes,
    int SendToPayLimitMinutes,
    DateTimeOffset CreatedAt,
    long CreatedBy,
    DateTimeOffset? UpdatedAt,
    long? UpdatedBy);
