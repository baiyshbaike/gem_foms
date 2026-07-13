using Application.Audit;
using Application.Regions;
using Contracts.Regions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Regions;

public sealed class DistrictService : IDistrictService
{
    private readonly AppDbContext _db;
    private readonly IActionLogService _actionLogService;

    public DistrictService(AppDbContext db, IActionLogService actionLogService)
    {
        _db = db;
        _actionLogService = actionLogService;
    }

    public async Task<IReadOnlyList<DistrictDto>> GetAsync(
        long? regionId,
        bool includeInactive,
        CancellationToken cancellationToken)
    {
        var query = DistrictQuery(includeInactive);

        if (regionId is not null)
        {
            query = query.Where(x => x.RegionId == regionId);
        }

        var districts = await query
            .OrderBy(x => x.Region.Name)
            .ThenBy(x => x.Name)
            .Select(x => new DistrictDto(
                x.Id,
                x.RegionId,
                x.Region.Name,
                x.Name,
                x.IsActive))
            .ToListAsync(cancellationToken);

        await AddLogAsync(
            "DistrictsViewed",
            null,
            200,
            true,
            null,
            $$"""{"regionId":{{(regionId?.ToString() ?? "null")}},"includeInactive":{{includeInactive.ToString().ToLowerInvariant()}},"resultCount":{{districts.Count}}}""",
            cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return districts;
    }

    public async Task<DistrictDto?> GetByIdAsync(
        long id,
        bool includeInactive,
        CancellationToken cancellationToken)
    {
        var district = await DistrictQuery(includeInactive)
            .Where(x => x.Id == id)
            .Select(x => new DistrictDto(
                x.Id,
                x.RegionId,
                x.Region.Name,
                x.Name,
                x.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

        await AddLogAsync(
            district is null ? "DistrictViewFailed" : "DistrictViewed",
            id.ToString(),
            district is null ? 404 : 200,
            district is not null,
            district is null ? "District not found" : null,
            $$"""{"includeInactive":{{includeInactive.ToString().ToLowerInvariant()}}}""",
            cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return district;
    }

    private IQueryable<Domain.Regions.District> DistrictQuery(bool includeInactive)
    {
        return _db.Districts
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted &&
                !x.Region.IsDeleted &&
                (includeInactive || (x.IsActive && x.Region.IsActive)));
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
            EntityName = "District",
            EntityId = entityId,
            StatusCode = statusCode,
            Succeeded = succeeded,
            FailureReason = failureReason,
            MetadataJson = metadataJson
        }, cancellationToken);
    }
}
