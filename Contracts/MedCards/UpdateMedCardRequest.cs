namespace Contracts.MedCards;

public sealed record UpdateMedCardRequest(
    string CardNumber,
    DateTimeOffset OpenedAt,
    DateTimeOffset? ClosedAt,
    MedCardStatusDto Status,
    string? Notes);