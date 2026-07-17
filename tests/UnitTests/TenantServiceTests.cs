using Application.Audit;
using Application.Auth;
using Application.Tenants;
using Contracts.Tenants;
using Domain.Sessions;
using Domain.Tenants;
using Domain.Users;
using Infrastructure.Data;
using Infrastructure.Tenants;
using Microsoft.EntityFrameworkCore;
using GeoDistrict = Domain.Regions.District;
using GeoRegion = Domain.Regions.Region;

namespace UnitTests;

public sealed class TenantServiceTests
{
    [Fact]
    public async Task QueryGrid_applies_remote_operations_and_grouping()
    {
        await using var context = CreateContext();
        await SeedAsync(context);
        var audit = new RecordingActionLogService();
        var service = CreateService(context, audit);

        var result = await service.QueryGridAsync(1, new TenantGridQueryRequest
        {
            Page = 1,
            PageSize = 1,
            Search = "center",
            Filters =
            [
                new TenantGridFilterDto
                {
                    Field = "isActive",
                    Operator = "equals",
                    Value = "true"
                }
            ],
            Sorting =
            [
                new TenantGridSortDto { Field = "name", Descending = true }
            ],
            GroupBy = "regionName"
        }, CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("Alpha Center", result.Items[0].Name);
        Assert.Equal(2, result.Groups.Sum(group => group.Count));
        Assert.Contains(audit.Entries, entry => entry.Action == "TenantGridQueried");
    }

    [Fact]
    public async Task ExportGrid_returns_only_selected_rows()
    {
        await using var context = CreateContext();
        await SeedAsync(context);
        var service = CreateService(context);

        var result = await service.ExportGridAsync(1, new TenantGridExportRequest
        {
            SelectedIds = ["tenant-alpha", "tenant-gamma"],
            Sorting = [new TenantGridSortDto { Field = "code" }]
        }, CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(["ALPHA", "GAMMA"], result.Items.Select(item => item.Code).ToArray());
    }

    [Fact]
    public async Task Create_normalizes_code_and_rejects_duplicate_or_invalid_location()
    {
        await using var context = CreateContext();
        await SeedAsync(context);
        var audit = new RecordingActionLogService();
        var service = CreateService(context, audit);

        var duplicate = await service.CreateAsync(1, ValidCreateRequest(" alpha "), CancellationToken.None);
        Assert.Equal(TenantCommandStatus.Conflict, duplicate.Status);

        var invalidLocationRequest = ValidCreateRequest("DELTA");
        invalidLocationRequest.RegionId = 2;
        invalidLocationRequest.DistrictId = 1;
        var invalidLocation = await service.CreateAsync(1, invalidLocationRequest, CancellationToken.None);
        Assert.Equal(TenantCommandStatus.ValidationFailed, invalidLocation.Status);

        var created = await service.CreateAsync(1, ValidCreateRequest(" delta-01 "), CancellationToken.None);
        Assert.Equal(TenantCommandStatus.Succeeded, created.Status);
        Assert.Equal("DELTA-01", created.Value!.Code);
        Assert.Equal(32, created.Value.Id.Length);
        Assert.Contains(audit.Entries, entry => entry.Action == "TenantCreated");
    }

    [Fact]
    public async Task Deactivate_blocks_active_session_and_update_can_reactivate()
    {
        await using var context = CreateContext();
        await SeedAsync(context);
        context.HdSessions.Add(new Domain.Sessions.HdSession
        {
            TenantId = "tenant-alpha",
            PatientId = 1,
            MedCardId = 1,
            Status = SessionStatus.Started,
            IdentifiedAt = DateTimeOffset.UtcNow.AddHours(-1),
            StartedAt = DateTimeOffset.UtcNow.AddMinutes(-30),
            StatusChangedAt = DateTimeOffset.UtcNow.AddMinutes(-30)
        });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var blocked = await service.DeactivateAsync(1, "tenant-alpha", CancellationToken.None);
        Assert.Equal(TenantCommandStatus.Conflict, blocked.Status);

        context.HdSessions.RemoveRange(context.HdSessions);
        await context.SaveChangesAsync();
        var deactivated = await service.DeactivateAsync(1, "tenant-alpha", CancellationToken.None);
        Assert.Equal(TenantCommandStatus.Succeeded, deactivated.Status);
        Assert.False(deactivated.Value!.IsActive);
        Assert.NotNull(deactivated.Value.DisabledAt);

        var reactivated = await service.UpdateAsync(
            1,
            "tenant-alpha",
            ValidUpdateRequest(deactivated.Value, isActive: true),
            CancellationToken.None);
        Assert.Equal(TenantCommandStatus.Succeeded, reactivated.Status);
        Assert.True(reactivated.Value!.IsActive);
        Assert.Null(reactivated.Value.DisabledAt);
    }

    private static TenantService CreateService(
        AppDbContext context,
        RecordingActionLogService? actionLog = null)
    {
        return new TenantService(
            context,
            new FakeJwtTokenService(),
            actionLog ?? new RecordingActionLogService(),
            new FakeTenantAccessService(context));
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new AppDbContext(options);
    }

    private static async Task SeedAsync(AppDbContext context)
    {
        var geoNorth = new GeoRegion { Id = 1, Name = "North", IsActive = true };
        var geoSouth = new GeoRegion { Id = 2, Name = "South", IsActive = true };
        var northDistrict = new GeoDistrict
        {
            Id = 1,
            Name = "North District",
            RegionId = 1,
            Region = geoNorth,
            IsActive = true
        };
        var southDistrict = new GeoDistrict
        {
            Id = 2,
            Name = "South District",
            RegionId = 2,
            Region = geoSouth,
            IsActive = true
        };

        context.Regions.AddRange(geoNorth, geoSouth);
        context.Districts.AddRange(northDistrict, southDistrict);
        context.Tenants.AddRange(
            CreateTenant("tenant-alpha", "ALPHA", "Alpha Center", 1, 1, true),
            CreateTenant("tenant-beta", "BETA", "Beta Center", 1, 1, false),
            CreateTenant("tenant-gamma", "GAMMA", "Gamma Center", 2, 2, true));
        await context.SaveChangesAsync();
    }

    private static Tenant CreateTenant(
        string id,
        string code,
        string name,
        long regionId,
        long districtId,
        bool isActive)
    {
        return new Tenant
        {
            Id = id,
            Code = code,
            Name = name,
            Address = $"{name} address",
            Phone = "+996 312 00 00 00",
            RegionId = regionId,
            DistrictId = districtId,
            IsActive = isActive,
            CreatedAt = DateTimeOffset.UtcNow,
            DisabledAt = isActive ? null : DateTimeOffset.UtcNow
        };
    }

    private static CreateTenantRequest ValidCreateRequest(string code)
    {
        return new CreateTenantRequest
        {
            Code = code,
            Name = "Delta Center",
            Phone = "+996 312 11 11 11",
            RegionId = 1,
            DistrictId = 1
        };
    }

    private static UpdateTenantRequest ValidUpdateRequest(TenantDetailsDto tenant, bool isActive)
    {
        return new UpdateTenantRequest
        {
            Code = tenant.Code,
            Name = tenant.Name,
            Address = tenant.Address,
            Phone = tenant.Phone,
            RegionId = tenant.RegionId,
            DistrictId = tenant.DistrictId,
            IsActive = isActive
        };
    }

    private sealed class RecordingActionLogService : IActionLogService
    {
        public List<ActionLogRequest> Entries { get; } = [];

        public Task AddAsync(ActionLogRequest request, CancellationToken cancellationToken)
        {
            Entries.Add(request);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeJwtTokenService : IJwtTokenService
    {
        public (string Token, DateTimeOffset ExpiresAt) CreateAccessToken(
            User user,
            IReadOnlyCollection<string> permissions,
            string? activeTenantId = null)
        {
            return ("token", DateTimeOffset.UtcNow.AddMinutes(30));
        }
    }

    private sealed class FakeTenantAccessService : ITenantAccessService
    {
        private readonly AppDbContext _context;

        public FakeTenantAccessService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<TenantDto>> GetAccessibleTenantsAsync(
            long userId,
            CancellationToken cancellationToken)
        {
            return await _context.Tenants
                .Where(tenant => tenant.IsActive)
                .OrderBy(tenant => tenant.Name)
                .Select(tenant => new TenantDto(tenant.Id, tenant.Code, tenant.Name))
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<string>> GetAccessibleTenantIdsAsync(
            long userId,
            CancellationToken cancellationToken)
        {
            return await _context.Tenants
                .Where(tenant => tenant.IsActive)
                .Select(tenant => tenant.Id)
                .ToListAsync(cancellationToken);
        }

        public Task<bool> CanAccessTenantAsync(
            long userId,
            string tenantId,
            CancellationToken cancellationToken)
        {
            return _context.Tenants.AnyAsync(
                tenant => tenant.Id == tenantId && tenant.IsActive,
                cancellationToken);
        }

        public async Task<TenantFilterResult> ResolveTenantFilterAsync(
            long userId,
            IReadOnlyList<string>? requestedTenantIds,
            CancellationToken cancellationToken)
        {
            var ids = await GetAccessibleTenantIdsAsync(userId, cancellationToken);
            return new TenantFilterResult(true, requestedTenantIds ?? ids);
        }
    }
}
