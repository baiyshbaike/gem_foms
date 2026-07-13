namespace Contracts.Regions;

public sealed record DistrictDto(
    long Id,
    long RegionId,
    string RegionName,
    string Name,
    bool IsActive);
