using Contracts.Regions;

namespace Application.Regions;

public interface IDistrictService
{
    Task<IReadOnlyList<DistrictDto>> GetAsync(
        long? regionId,
        bool includeInactive,
        CancellationToken cancellationToken);

    Task<DistrictDto?> GetByIdAsync(
        long id,
        bool includeInactive,
        CancellationToken cancellationToken);
}
