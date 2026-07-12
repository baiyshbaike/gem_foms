namespace Application.MedCards;

public sealed record MedCardCommandResult<T>(
    MedCardCommandStatus Status,
    T? Value = default);