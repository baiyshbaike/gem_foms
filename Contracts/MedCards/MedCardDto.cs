namespace Contracts.MedCards;

public sealed record MedCardDto(
    long Id,
    string TenantId,
    long PatientId,
    string CardNumber,
    DateTimeOffset OpenedAt,
    DateTimeOffset? ClosedAt,
    MedCardStatusDto Status,
    string? Notes);