using Contracts.MedCards;

namespace Application.MedCards;

public interface IMedCardService
{
    Task<IReadOnlyList<MedCardDto>?> GetAsync(
        long userId,
        IReadOnlyList<string>? tenantIds,
        CancellationToken cancellationToken);

    Task<MedCardDto?> GetByIdAsync(
        long userId,
        string tenantId,
        long id,
        CancellationToken cancellationToken);

    Task<MedCardCommandResult<MedCardDto>> CreateAsync(
        string tenantId,
        long userId,
        CreateMedCardRequest request,
        CancellationToken cancellationToken);

    Task<MedCardCommandResult<MedCardDto>> UpdateAsync(
        string tenantId,
        long id,
        long userId,
        UpdateMedCardRequest request,
        CancellationToken cancellationToken);

    Task<MedCardCommandResult<bool>> DeleteAsync(
        string tenantId,
        long id,
        long userId,
        CancellationToken cancellationToken);
}