namespace Contracts.Regions;

public sealed record RegionDto(
    long Id,
    string Name,
    bool IsActive,
    IReadOnlyList<DistrictDto> Districts);
