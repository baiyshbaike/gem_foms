using Contracts.Regions;

namespace Application.Regions;

public interface IRegionService
{
    Task<IReadOnlyList<RegionDto>> GetAsync(
        bool includeInactive,
        CancellationToken cancellationToken);

    Task<RegionDto?> GetByIdAsync(
        long id,
        bool includeInactive,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<DistrictDto>?> GetDistrictsAsync(
        long regionId,
        bool includeInactive,
        CancellationToken cancellationToken);
}
