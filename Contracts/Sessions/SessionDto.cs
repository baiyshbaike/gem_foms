namespace Contracts.Sessions;

public sealed record SessionDto(
    long Id,
    string TenantId,
    long PatientId,
    long MedCardId,
    long? MachineId,
    string Status,
    DateTimeOffset IdentifiedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt,
    DateTimeOffset? EndIdentifiedAt,
    DateTimeOffset? SentToPayAt,
    DateTimeOffset? PaidAt,
    int? ActiveMinutes,
    int? PauseMinutes);