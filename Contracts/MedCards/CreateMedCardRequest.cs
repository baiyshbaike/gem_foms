namespace Contracts.MedCards;

public sealed record CreateMedCardRequest(
    long PatientId,
    string CardNumber,
    DateTimeOffset? OpenedAt,
    string? Notes);