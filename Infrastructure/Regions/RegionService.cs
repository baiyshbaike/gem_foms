using Application.Audit;
using Application.Regions;
using Contracts.Regions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Regions;

public sealed class RegionService : IRegionService
{
    private readonly AppDbContext _db;
    private readonly IActionLogService _actionLogService;

    public RegionService(AppDbContext db, IActionLogService actionLogService)
    {
        _db = db;
        _actionLogService = actionLogService;
    }

    public async Task<IReadOnlyList<RegionDto>> GetAsync(
        bool includeInactive,
        CancellationToken cancellationToken)
    {
        var regions = await RegionQuery(includeInactive)
            .OrderBy(x => x.Name)
            .Select(region => new RegionDto(
                region.Id,
                region.Name,
                region.IsActive,
                region.Districts
                    .Where(district => !district.IsDeleted && (includeInactive || district.IsActive))
                    .OrderBy(district => district.Name)
                    .Select(district => new DistrictDto(
                        district.Id,
                        district.RegionId,
                        region.Name,
                        district.Name,
                        district.IsActive))
                    .ToList()))
            .ToListAsync(cancellationToken);

        await AddLogAsync(
            "RegionsViewed",
            null,
            200,
            true,
            null,
            $$"""{"includeInactive":{{includeInactive.ToString().ToLowerInvariant()}},"resultCount":{{regions.Count}}}""",
            cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return regions;
    }

    public async Task<RegionDto?> GetByIdAsync(
        long id,
        bool includeInactive,
        CancellationToken cancellationToken)
    {
        var region = await RegionQuery(includeInactive)
            .Where(x => x.Id == id)
            .Select(x => new RegionDto(
                x.Id,
                x.Name,
                x.IsActive,
                x.Districts
                    .Where(district => !district.IsDeleted && (includeInactive || district.IsActive))
                    .OrderBy(district => district.Name)
                    .Select(district => new DistrictDto(
                        district.Id,
                        district.RegionId,
                        x.Name,
                        district.Name,
                        district.IsActive))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);

        await AddLogAsync(
            region is null ? "RegionViewFailed" : "RegionViewed",
            id.ToString(),
            region is null ? 404 : 200,
            region is not null,
            region is null ? "Region not found" : null,
            $$"""{"includeInactive":{{includeInactive.ToString().ToLowerInvariant()}}}""",
            cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return region;
    }

    public async Task<IReadOnlyList<DistrictDto>?> GetDistrictsAsync(
        long regionId,
        bool includeInactive,
        CancellationToken cancellationToken)
    {
        var regionExists = await RegionQuery(includeInactive)
            .AnyAsync(x => x.Id == regionId, cancellationToken);

        if (!regionExists)
        {
            await AddLogAsync(
                "RegionDistrictsViewFailed",
                regionId.ToString(),
                404,
                false,
                "Region not found",
                $$"""{"regionId":{{regionId}},"includeInactive":{{includeInactive.ToString().ToLowerInvariant()}}}""",
                cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return null;
        }

        var districts = await _db.Districts
            .AsNoTracking()
            .Where(x =>
                x.RegionId == regionId &&
                !x.IsDeleted &&
                (includeInactive || x.IsActive))
            .OrderBy(x => x.Name)
            .Select(x => new DistrictDto(
                x.Id,
                x.RegionId,
                x.Region.Name,
                x.Name,
                x.IsActive))
            .ToListAsync(cancellationToken);

        await AddLogAsync(
            "RegionDistrictsViewed",
            regionId.ToString(),
            200,
            true,
            null,
            $$"""{"regionId":{{regionId}},"includeInactive":{{includeInactive.ToString().ToLowerInvariant()}},"resultCount":{{districts.Count}}}""",
            cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return districts;
    }

    private IQueryable<Domain.Regions.Region> RegionQuery(bool includeInactive)
    {
        return _db.GeoRegions
            .AsNoTracking()
            .Where(x => !x.IsDeleted && (includeInactive || x.IsActive));
    }

    private async Task AddLogAsync(
        string action,
        string? entityId,
        int statusCode,
        bool succeeded,
        string? failureReason,
        string? metadataJson,
        CancellationToken cancellationToken)
    {
        await _actionLogService.AddAsync(new ActionLogRequest
        {
            Action = action,
            Module = "region",
            EntityName = "GeoRegion",
            EntityId = entityId,
            StatusCode = statusCode,
            Succeeded = succeeded,
            FailureReason = failureReason,
            MetadataJson = metadataJson
        }, cancellationToken);
    }
}
