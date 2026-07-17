using System.Globalization;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.RegularExpressions;
using Application.Audit;
using Application.Auth;
using Application.Tenants;
using Contracts.Tenants;
using Domain.Sessions;
using Domain.Tenants;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tenants;

public sealed class TenantService : ITenantService
{
    private const int DefaultGridPageSize = 25;
    private const int MaxGridPageSize = 100;
    private const int MaxExportRows = 50_000;

    private readonly AppDbContext _db;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IActionLogService _actionLogService;
    private readonly ITenantAccessService _tenantAccessService;

    public TenantService(
        AppDbContext db,
        IJwtTokenService jwtTokenService,
        IActionLogService actionLogService,
        ITenantAccessService tenantAccessService)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
        _actionLogService = actionLogService;
        _tenantAccessService = tenantAccessService;
    }

    public async Task<TenantGridQueryResult> QueryGridAsync(
        long userId,
        TenantGridQueryRequest request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(
            request.PageSize <= 0 ? DefaultGridPageSize : request.PageSize,
            1,
            MaxGridPageSize);
        var query = ApplyGridRequest(ProjectToGridRow(GetTenantQuery()), request);
        var totalCount = await query.CountAsync(cancellationToken);
        var groups = await LoadGroupSummariesAsync(query, request.GroupBy, cancellationToken);
        var items = await ApplySorting(query, request.Sorting, request.GroupBy)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        await AddGridActionLogAsync(
            userId,
            "TenantGridQueried",
            request,
            page,
            pageSize,
            selectedCount: 0,
            cancellationToken);

        return new TenantGridQueryResult(items, totalCount, groups);
    }

    public async Task<TenantGridQueryResult> ExportGridAsync(
        long userId,
        TenantGridExportRequest request,
        CancellationToken cancellationToken)
    {
        var query = ApplyGridRequest(ProjectToGridRow(GetTenantQuery()), request);
        var selectedIds = request.SelectedIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .Distinct(StringComparer.Ordinal)
            .Take(MaxExportRows)
            .ToArray();

        if (selectedIds.Length > 0)
        {
            query = query.Where(tenant => selectedIds.Contains(tenant.Id));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var groups = await LoadGroupSummariesAsync(query, request.GroupBy, cancellationToken);
        var items = await ApplySorting(query, request.Sorting, request.GroupBy)
            .Take(MaxExportRows)
            .ToListAsync(cancellationToken);

        await AddGridActionLogAsync(
            userId,
            "TenantGridExported",
            request,
            page: 1,
            take: items.Count,
            selectedIds.Length,
            cancellationToken);

        return new TenantGridQueryResult(items, totalCount, groups);
    }

    public async Task<TenantDetailsDto?> GetByIdAsync(
        long userId,
        string tenantId,
        CancellationToken cancellationToken)
    {
        var tenant = await LoadDetailsAsync(tenantId, cancellationToken);

        await AddActionLogAsync(
            userId,
            tenant is null ? "TenantViewFailed" : "TenantViewed",
            tenantId,
            tenant is null ? 404 : 200,
            tenant is not null,
            tenant is null ? "Tenant not found" : null,
            metadata: null,
            cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return tenant;
    }

    public async Task<TenantCommandResult<TenantDetailsDto>> CreateAsync(
        long userId,
        CreateTenantRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await ValidateAndNormalizeAsync(request, cancellationToken);
        if (validation.Value is null)
        {
            await SaveFailureLogAsync(
                userId,
                "TenantCreateFailed",
                entityId: null,
                statusCode: 400,
                validation.Error!,
                cancellationToken);
            return new TenantCommandResult<TenantDetailsDto>(
                TenantCommandStatus.ValidationFailed,
                Error: validation.Error);
        }

        var input = validation.Value;
        if (await TenantCodeExistsAsync(input.Code, excludedTenantId: null, cancellationToken))
        {
            const string error = "A tenant with the same code already exists.";
            await SaveFailureLogAsync(userId, "TenantCreateFailed", null, 409, error, cancellationToken);
            return new TenantCommandResult<TenantDetailsDto>(TenantCommandStatus.Conflict, Error: error);
        }

        var tenant = new Tenant
        {
            Id = Guid.CreateVersion7().ToString("N"),
            Code = input.Code,
            Name = input.Name,
            Address = input.Address,
            Phone = input.Phone,
            RegionId = input.RegionId,
            DistrictId = input.DistrictId,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.Tenants.Add(tenant);
        await AddActionLogAsync(
            userId,
            "TenantCreated",
            tenant.Id,
            201,
            succeeded: true,
            failureReason: null,
            metadata: null,
            cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        var created = await LoadDetailsAsync(tenant.Id, cancellationToken);
        return new TenantCommandResult<TenantDetailsDto>(TenantCommandStatus.Succeeded, created);
    }

    public async Task<TenantCommandResult<TenantDetailsDto>> UpdateAsync(
        long userId,
        string tenantId,
        UpdateTenantRequest request,
        CancellationToken cancellationToken)
    {
        var tenant = await _db.Tenants
            .FirstOrDefaultAsync(item => item.Id == tenantId, cancellationToken);
        if (tenant is null)
        {
            const string error = "Tenant not found.";
            await SaveFailureLogAsync(userId, "TenantUpdateFailed", tenantId, 404, error, cancellationToken);
            return new TenantCommandResult<TenantDetailsDto>(TenantCommandStatus.NotFound, Error: error);
        }

        var validation = await ValidateAndNormalizeAsync(request, cancellationToken);
        if (validation.Value is null)
        {
            await SaveFailureLogAsync(
                userId,
                "TenantUpdateFailed",
                tenantId,
                400,
                validation.Error!,
                cancellationToken);
            return new TenantCommandResult<TenantDetailsDto>(
                TenantCommandStatus.ValidationFailed,
                Error: validation.Error);
        }

        var input = validation.Value;
        if (await TenantCodeExistsAsync(input.Code, tenantId, cancellationToken))
        {
            const string error = "A tenant with the same code already exists.";
            await SaveFailureLogAsync(userId, "TenantUpdateFailed", tenantId, 409, error, cancellationToken);
            return new TenantCommandResult<TenantDetailsDto>(TenantCommandStatus.Conflict, Error: error);
        }

        var isDeactivating = tenant.IsActive && !request.IsActive;
        var isReactivating = !tenant.IsActive && request.IsActive;
        if (isDeactivating && await HasActiveSessionsAsync(tenantId, cancellationToken))
        {
            const string error = "Tenant cannot be deactivated while it has an active session.";
            await SaveFailureLogAsync(userId, "TenantDeactivateFailed", tenantId, 409, error, cancellationToken);
            return new TenantCommandResult<TenantDetailsDto>(TenantCommandStatus.Conflict, Error: error);
        }

        tenant.Code = input.Code;
        tenant.Name = input.Name;
        tenant.Address = input.Address;
        tenant.Phone = input.Phone;
        tenant.RegionId = input.RegionId;
        tenant.DistrictId = input.DistrictId;
        tenant.IsActive = request.IsActive;
        tenant.DisabledAt = request.IsActive ? null : tenant.DisabledAt ?? DateTimeOffset.UtcNow;

        var action = isDeactivating
            ? "TenantDeactivated"
            : isReactivating
                ? "TenantReactivated"
                : "TenantUpdated";
        await AddActionLogAsync(
            userId,
            action,
            tenant.Id,
            200,
            succeeded: true,
            failureReason: null,
            metadata: null,
            cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        var updated = await LoadDetailsAsync(tenant.Id, cancellationToken);
        return new TenantCommandResult<TenantDetailsDto>(TenantCommandStatus.Succeeded, updated);
    }

    public async Task<TenantCommandResult<TenantDetailsDto>> DeactivateAsync(
        long userId,
        string tenantId,
        CancellationToken cancellationToken)
    {
        var tenant = await _db.Tenants
            .FirstOrDefaultAsync(item => item.Id == tenantId, cancellationToken);
        if (tenant is null)
        {
            const string error = "Tenant not found.";
            await SaveFailureLogAsync(userId, "TenantDeactivateFailed", tenantId, 404, error, cancellationToken);
            return new TenantCommandResult<TenantDetailsDto>(TenantCommandStatus.NotFound, Error: error);
        }

        if (!tenant.IsActive)
        {
            var existing = await LoadDetailsAsync(tenant.Id, cancellationToken);
            return new TenantCommandResult<TenantDetailsDto>(TenantCommandStatus.Succeeded, existing);
        }

        if (await HasActiveSessionsAsync(tenantId, cancellationToken))
        {
            const string error = "Tenant cannot be deactivated while it has an active session.";
            await SaveFailureLogAsync(userId, "TenantDeactivateFailed", tenantId, 409, error, cancellationToken);
            return new TenantCommandResult<TenantDetailsDto>(TenantCommandStatus.Conflict, Error: error);
        }

        tenant.IsActive = false;
        tenant.DisabledAt = DateTimeOffset.UtcNow;
        await AddActionLogAsync(
            userId,
            "TenantDeactivated",
            tenant.Id,
            204,
            succeeded: true,
            failureReason: null,
            metadata: null,
            cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        var deactivated = await LoadDetailsAsync(tenant.Id, cancellationToken);
        return new TenantCommandResult<TenantDetailsDto>(TenantCommandStatus.Succeeded, deactivated);
    }

    public async Task<IReadOnlyList<TenantDto>> GetMyTenantsAsync(
        long userId,
        CancellationToken cancellationToken)
    {
        var tenants = await _tenantAccessService.GetAccessibleTenantsAsync(userId, cancellationToken);

        await _actionLogService.AddAsync(new ActionLogRequest
        {
            UserId = userId,
            Action = "MyTenantsViewed",
            Module = "tenant",
            EntityName = "Tenant",
            StatusCode = 200,
            Succeeded = true,
            MetadataJson = JsonSerializer.Serialize(new { resultCount = tenants.Count })
        }, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return tenants;
    }

    public async Task<SwitchTenantResponse?> SwitchAsync(
        long userId,
        string tenantId,
        CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(item => item.Id == userId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            await AddActionLogAsync(
                userId,
                "TenantSwitchFailed",
                tenantId,
                401,
                succeeded: false,
                "User not found or inactive",
                metadata: null,
                cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return null;
        }

        if (!await _tenantAccessService.CanAccessTenantAsync(userId, tenantId, cancellationToken))
        {
            await AddActionLogAsync(
                userId,
                "TenantSwitchFailed",
                tenantId,
                403,
                succeeded: false,
                "Tenant access denied",
                metadata: null,
                cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return null;
        }

        var tenant = await _db.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == tenantId && item.IsActive, cancellationToken);
        if (tenant is null)
        {
            await AddActionLogAsync(
                userId,
                "TenantSwitchFailed",
                tenantId,
                404,
                succeeded: false,
                "Tenant not found or inactive",
                metadata: null,
                cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return null;
        }

        var permissions = await _db.UserRoles
            .AsNoTracking()
            .Where(userRole => userRole.UserId == user.Id)
            .SelectMany(userRole => userRole.Role.RolePermissions)
            .Select(rolePermission => rolePermission.Permission.Code)
            .Distinct()
            .ToListAsync(cancellationToken);
        var token = _jwtTokenService.CreateAccessToken(user, permissions, tenant.Id);

        await AddActionLogAsync(
            userId,
            "TenantSwitchSucceeded",
            tenant.Id,
            200,
            succeeded: true,
            failureReason: null,
            metadata: null,
            cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return new SwitchTenantResponse(
            token.Token,
            token.ExpiresAt,
            new TenantDto(tenant.Id, tenant.Code, tenant.Name));
    }

    private async Task<TenantInputValidationResult> ValidateAndNormalizeAsync(
        CreateTenantRequest request,
        CancellationToken cancellationToken)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        if (code.Length == 0 || !Regex.IsMatch(code, "^[A-Z0-9][A-Z0-9_-]*$", RegexOptions.CultureInvariant))
        {
            return TenantInputValidationResult.Failed(
                "Tenant code may contain only Latin letters, numbers, hyphens, and underscores.");
        }

        var name = request.Name.Trim();
        if (name.Length == 0)
        {
            return TenantInputValidationResult.Failed("Tenant name is required.");
        }

        var phone = request.Phone?.Trim() ?? string.Empty;
        if (phone.Length == 0)
        {
            return TenantInputValidationResult.Failed("Tenant phone is required.");
        }

        var locationExists = await _db.Districts
            .AsNoTracking()
            .AnyAsync(
                district => district.Id == request.DistrictId
                    && district.RegionId == request.RegionId
                    && district.IsActive
                    && !district.IsDeleted
                    && district.Region.IsActive
                    && !district.Region.IsDeleted,
                cancellationToken);
        if (!locationExists)
        {
            return TenantInputValidationResult.Failed(
                "District does not belong to the selected region or is inactive.");
        }

        return TenantInputValidationResult.Succeeded(new TenantInput(
            code,
            name,
            NormalizeOptional(request.Address),
            phone,
            request.RegionId,
            request.DistrictId));
    }

    private Task<bool> TenantCodeExistsAsync(
        string code,
        string? excludedTenantId,
        CancellationToken cancellationToken)
    {
        return _db.Tenants
            .AsNoTracking()
            .AnyAsync(
                tenant => tenant.Code == code
                    && (excludedTenantId == null || tenant.Id != excludedTenantId),
                cancellationToken);
    }

    private Task<bool> HasActiveSessionsAsync(string tenantId, CancellationToken cancellationToken)
    {
        return _db.HdSessions
            .AsNoTracking()
            .AnyAsync(
                session => session.TenantId == tenantId
                    && (session.Status == SessionStatus.Identified
                        || session.Status == SessionStatus.Started
                        || session.Status == SessionStatus.Paused),
                cancellationToken);
    }

    private IQueryable<Tenant> GetTenantQuery()
    {
        return _db.Tenants.AsNoTracking();
    }

    private Task<TenantDetailsDto?> LoadDetailsAsync(
        string tenantId,
        CancellationToken cancellationToken)
    {
        return ProjectToDetails(GetTenantQuery().Where(tenant => tenant.Id == tenantId))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static IQueryable<TenantDetailsDto> ProjectToDetails(IQueryable<Tenant> query)
    {
        return query.Select(tenant => new TenantDetailsDto(
            tenant.Id,
            tenant.Code,
            tenant.Name,
            tenant.Address,
            tenant.Phone,
            tenant.RegionId,
            tenant.Region.Name,
            tenant.DistrictId,
            tenant.District.Name,
            tenant.IsActive,
            tenant.CreatedAt,
            tenant.DisabledAt));
    }

    private static IQueryable<TenantGridRowDto> ProjectToGridRow(IQueryable<Tenant> query)
    {
        return query.Select(tenant => new TenantGridRowDto
        {
            Id = tenant.Id,
            Code = tenant.Code,
            Name = tenant.Name,
            Address = tenant.Address,
            Phone = tenant.Phone,
            RegionId = tenant.RegionId,
            RegionName = tenant.Region.Name,
            DistrictId = tenant.DistrictId,
            DistrictName = tenant.District.Name,
            IsActive = tenant.IsActive,
            CreatedAt = tenant.CreatedAt,
            DisabledAt = tenant.DisabledAt
        });
    }

    private static IQueryable<TenantGridRowDto> ApplyGridRequest(
        IQueryable<TenantGridRowDto> query,
        TenantGridQueryRequest request)
    {
        var search = request.Search?.Trim().ToLowerInvariant();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(tenant =>
                tenant.Code.ToLower().Contains(search)
                || tenant.Name.ToLower().Contains(search)
                || (tenant.Address ?? string.Empty).ToLower().Contains(search)
                || (tenant.Phone ?? string.Empty).ToLower().Contains(search)
                || tenant.RegionName.ToLower().Contains(search)
                || tenant.DistrictName.ToLower().Contains(search));
        }

        foreach (var filter in (request.Filters ?? []).Take(20))
        {
            query = ApplyFilter(query, filter);
        }

        return query;
    }

    private static IQueryable<TenantGridRowDto> ApplyFilter(
        IQueryable<TenantGridRowDto> query,
        TenantGridFilterDto filter)
    {
        return NormalizeGridField(filter.Field) switch
        {
            "id" => ApplyStringFilter(query, filter, tenant => tenant.Id),
            "code" => ApplyStringFilter(query, filter, tenant => tenant.Code),
            "name" => ApplyStringFilter(query, filter, tenant => tenant.Name),
            "address" => ApplyStringFilter(query, filter, tenant => tenant.Address),
            "phone" => ApplyStringFilter(query, filter, tenant => tenant.Phone),
            "regionId" => ApplyLongFilter(query, filter, tenant => tenant.RegionId),
            "regionName" => ApplyStringFilter(query, filter, tenant => tenant.RegionName),
            "districtId" => ApplyLongFilter(query, filter, tenant => tenant.DistrictId),
            "districtName" => ApplyStringFilter(query, filter, tenant => tenant.DistrictName),
            "isActive" => ApplyBooleanFilter(query, filter),
            "createdAt" => ApplyDateTimeFilter(query, filter, useDisabledAt: false),
            "disabledAt" => ApplyDateTimeFilter(query, filter, useDisabledAt: true),
            _ => query
        };
    }

    private static IQueryable<TenantGridRowDto> ApplyStringFilter(
        IQueryable<TenantGridRowDto> query,
        TenantGridFilterDto filter,
        Expression<Func<TenantGridRowDto, string?>> selector)
    {
        var operation = NormalizeOperator(filter.Operator);
        var value = filter.Value?.Trim().ToLowerInvariant() ?? string.Empty;
        var normalizedSelector = Expression.Coalesce(selector.Body, Expression.Constant(string.Empty));
        var loweredSelector = Expression.Call(
            normalizedSelector,
            nameof(string.ToLower),
            Type.EmptyTypes);
        Expression? predicate = operation switch
        {
            "contains" when value.Length > 0 => Expression.Call(
                loweredSelector,
                nameof(string.Contains),
                Type.EmptyTypes,
                Expression.Constant(value)),
            "notContains" when value.Length > 0 => Expression.Not(Expression.Call(
                loweredSelector,
                nameof(string.Contains),
                Type.EmptyTypes,
                Expression.Constant(value))),
            "startsWith" when value.Length > 0 => Expression.Call(
                loweredSelector,
                nameof(string.StartsWith),
                Type.EmptyTypes,
                Expression.Constant(value)),
            "endsWith" when value.Length > 0 => Expression.Call(
                loweredSelector,
                nameof(string.EndsWith),
                Type.EmptyTypes,
                Expression.Constant(value)),
            "equals" => Expression.Equal(loweredSelector, Expression.Constant(value)),
            "notEquals" => Expression.NotEqual(loweredSelector, Expression.Constant(value)),
            "isEmpty" => Expression.Equal(normalizedSelector, Expression.Constant(string.Empty)),
            "isNotEmpty" => Expression.NotEqual(normalizedSelector, Expression.Constant(string.Empty)),
            _ => null
        };

        return predicate is null
            ? query
            : query.Where(Expression.Lambda<Func<TenantGridRowDto, bool>>(
                predicate,
                selector.Parameters));
    }

    private static IQueryable<TenantGridRowDto> ApplyLongFilter(
        IQueryable<TenantGridRowDto> query,
        TenantGridFilterDto filter,
        Expression<Func<TenantGridRowDto, long>> selector)
    {
        if (!long.TryParse(filter.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            return query;
        }

        var operation = NormalizeOperator(filter.Operator);
        var constant = Expression.Constant(value);
        Expression? predicate = operation switch
        {
            "equals" => Expression.Equal(selector.Body, constant),
            "notEquals" => Expression.NotEqual(selector.Body, constant),
            "greaterThan" => Expression.GreaterThan(selector.Body, constant),
            "greaterThanOrEqual" => Expression.GreaterThanOrEqual(selector.Body, constant),
            "lessThan" => Expression.LessThan(selector.Body, constant),
            "lessThanOrEqual" => Expression.LessThanOrEqual(selector.Body, constant),
            _ => null
        };

        return predicate is null
            ? query
            : query.Where(Expression.Lambda<Func<TenantGridRowDto, bool>>(
                predicate,
                selector.Parameters));
    }

    private static IQueryable<TenantGridRowDto> ApplyBooleanFilter(
        IQueryable<TenantGridRowDto> query,
        TenantGridFilterDto filter)
    {
        if (!bool.TryParse(filter.Value, out var value))
        {
            return query;
        }

        return NormalizeOperator(filter.Operator) switch
        {
            "equals" => query.Where(tenant => tenant.IsActive == value),
            "notEquals" => query.Where(tenant => tenant.IsActive != value),
            _ => query
        };
    }

    private static IQueryable<TenantGridRowDto> ApplyDateTimeFilter(
        IQueryable<TenantGridRowDto> query,
        TenantGridFilterDto filter,
        bool useDisabledAt)
    {
        var operation = NormalizeOperator(filter.Operator);
        if (useDisabledAt && operation == "isEmpty")
        {
            return query.Where(tenant => tenant.DisabledAt == null);
        }

        if (useDisabledAt && operation == "isNotEmpty")
        {
            return query.Where(tenant => tenant.DisabledAt != null);
        }

        if (!TryParseDate(filter.Value, out var date))
        {
            return query;
        }

        var start = new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var end = start.AddDays(1);
        if (useDisabledAt)
        {
            return operation switch
            {
                "equals" => query.Where(tenant => tenant.DisabledAt >= start && tenant.DisabledAt < end),
                "notEquals" => query.Where(tenant => tenant.DisabledAt < start || tenant.DisabledAt >= end),
                "greaterThan" => query.Where(tenant => tenant.DisabledAt >= end),
                "greaterThanOrEqual" => query.Where(tenant => tenant.DisabledAt >= start),
                "lessThan" => query.Where(tenant => tenant.DisabledAt < start),
                "lessThanOrEqual" => query.Where(tenant => tenant.DisabledAt < end),
                "between" when TryParseDate(filter.ValueTo, out var dateTo) =>
                    query.Where(tenant => tenant.DisabledAt >= start
                        && tenant.DisabledAt < new DateTimeOffset(
                            dateTo.AddDays(1).ToDateTime(TimeOnly.MinValue),
                            TimeSpan.Zero)),
                _ => query
            };
        }

        return operation switch
        {
            "equals" => query.Where(tenant => tenant.CreatedAt >= start && tenant.CreatedAt < end),
            "notEquals" => query.Where(tenant => tenant.CreatedAt < start || tenant.CreatedAt >= end),
            "greaterThan" => query.Where(tenant => tenant.CreatedAt >= end),
            "greaterThanOrEqual" => query.Where(tenant => tenant.CreatedAt >= start),
            "lessThan" => query.Where(tenant => tenant.CreatedAt < start),
            "lessThanOrEqual" => query.Where(tenant => tenant.CreatedAt < end),
            "between" when TryParseDate(filter.ValueTo, out var dateTo) =>
                query.Where(tenant => tenant.CreatedAt >= start
                    && tenant.CreatedAt < new DateTimeOffset(
                        dateTo.AddDays(1).ToDateTime(TimeOnly.MinValue),
                        TimeSpan.Zero)),
            _ => query
        };
    }

    private static IQueryable<TenantGridRowDto> ApplySorting(
        IQueryable<TenantGridRowDto> query,
        IReadOnlyList<TenantGridSortDto>? sorting,
        string? groupBy)
    {
        IOrderedQueryable<TenantGridRowDto>? ordered = null;
        var normalizedGroup = NormalizeGridField(groupBy);
        if (IsGroupField(normalizedGroup))
        {
            ordered = ApplySortField(query, ordered, normalizedGroup, descending: false);
        }

        foreach (var sort in (sorting ?? []).Take(10))
        {
            var field = NormalizeGridField(sort.Field);
            if (field == normalizedGroup)
            {
                continue;
            }

            ordered = ApplySortField(query, ordered, field, sort.Descending) ?? ordered;
        }

        if (ordered is null)
        {
            return query.OrderBy(tenant => tenant.Name).ThenBy(tenant => tenant.Code);
        }

        if (!(sorting ?? []).Any(sort => NormalizeGridField(sort.Field) == "id"))
        {
            ordered = ordered.ThenBy(tenant => tenant.Id);
        }

        return ordered;
    }

    private static IOrderedQueryable<TenantGridRowDto>? ApplySortField(
        IQueryable<TenantGridRowDto> query,
        IOrderedQueryable<TenantGridRowDto>? ordered,
        string? field,
        bool descending)
    {
        return field switch
        {
            "id" => ApplyOrder(query, ordered, tenant => tenant.Id, descending),
            "code" => ApplyOrder(query, ordered, tenant => tenant.Code, descending),
            "name" => ApplyOrder(query, ordered, tenant => tenant.Name, descending),
            "address" => ApplyOrder(query, ordered, tenant => tenant.Address, descending),
            "phone" => ApplyOrder(query, ordered, tenant => tenant.Phone, descending),
            "regionName" => ApplyOrder(query, ordered, tenant => tenant.RegionName, descending),
            "districtName" => ApplyOrder(query, ordered, tenant => tenant.DistrictName, descending),
            "isActive" => ApplyOrder(query, ordered, tenant => tenant.IsActive, descending),
            "createdAt" => ApplyOrder(query, ordered, tenant => tenant.CreatedAt, descending),
            "disabledAt" => ApplyOrder(query, ordered, tenant => tenant.DisabledAt, descending),
            _ => null
        };
    }

    private static IOrderedQueryable<TenantGridRowDto> ApplyOrder<TKey>(
        IQueryable<TenantGridRowDto> query,
        IOrderedQueryable<TenantGridRowDto>? ordered,
        Expression<Func<TenantGridRowDto, TKey>> selector,
        bool descending)
    {
        if (ordered is null)
        {
            return descending ? query.OrderByDescending(selector) : query.OrderBy(selector);
        }

        return descending ? ordered.ThenByDescending(selector) : ordered.ThenBy(selector);
    }

    private static async Task<IReadOnlyList<TenantGridGroupSummaryDto>> LoadGroupSummariesAsync(
        IQueryable<TenantGridRowDto> query,
        string? groupBy,
        CancellationToken cancellationToken)
    {
        return NormalizeGridField(groupBy) switch
        {
            "regionName" => await LoadStringGroupsAsync(
                query,
                tenant => tenant.RegionName,
                cancellationToken),
            "districtName" => await LoadStringGroupsAsync(
                query,
                tenant => tenant.DistrictName,
                cancellationToken),
            "isActive" => await LoadActiveGroupsAsync(query, cancellationToken),
            _ => []
        };
    }

    private static async Task<IReadOnlyList<TenantGridGroupSummaryDto>> LoadStringGroupsAsync(
        IQueryable<TenantGridRowDto> query,
        Expression<Func<TenantGridRowDto, string>> selector,
        CancellationToken cancellationToken)
    {
        var groups = await query
            .GroupBy(selector)
            .Select(group => new { Key = group.Key, Count = group.Count() })
            .OrderBy(group => group.Key)
            .ToListAsync(cancellationToken);
        return groups
            .Select(group => new TenantGridGroupSummaryDto(group.Key, group.Key, group.Count))
            .ToArray();
    }

    private static async Task<IReadOnlyList<TenantGridGroupSummaryDto>> LoadActiveGroupsAsync(
        IQueryable<TenantGridRowDto> query,
        CancellationToken cancellationToken)
    {
        var groups = await query
            .GroupBy(tenant => tenant.IsActive)
            .Select(group => new { Key = group.Key, Count = group.Count() })
            .OrderByDescending(group => group.Key)
            .ToListAsync(cancellationToken);
        return groups
            .Select(group => new TenantGridGroupSummaryDto(
                group.Key.ToString().ToLowerInvariant(),
                group.Key ? "Active" : "Inactive",
                group.Count))
            .ToArray();
    }

    private static string? NormalizeGridField(string? field)
    {
        return field?.Trim().ToLowerInvariant() switch
        {
            "id" => "id",
            "code" => "code",
            "name" => "name",
            "address" => "address",
            "phone" => "phone",
            "regionid" => "regionId",
            "regionname" => "regionName",
            "districtid" => "districtId",
            "districtname" => "districtName",
            "isactive" => "isActive",
            "createdat" => "createdAt",
            "disabledat" => "disabledAt",
            _ => null
        };
    }

    private static string NormalizeOperator(string? operation)
    {
        return operation?.Trim().ToLowerInvariant() switch
        {
            "notcontains" => "notContains",
            "startswith" => "startsWith",
            "endswith" => "endsWith",
            "notequals" => "notEquals",
            "greaterthan" => "greaterThan",
            "greaterthanorequal" => "greaterThanOrEqual",
            "lessthan" => "lessThan",
            "lessthanorequal" => "lessThanOrEqual",
            "isempty" => "isEmpty",
            "isnotempty" => "isNotEmpty",
            "between" => "between",
            "equals" => "equals",
            "contains" => "contains",
            _ => string.Empty
        };
    }

    private static bool IsGroupField(string? field)
    {
        return field is "regionName"
            or "districtName"
            or "isActive";
    }

    private static bool TryParseDate(string? value, out DateOnly date)
    {
        return DateOnly.TryParseExact(
            value,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out date);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private async Task AddGridActionLogAsync(
        long userId,
        string action,
        TenantGridQueryRequest request,
        int page,
        int take,
        int selectedCount,
        CancellationToken cancellationToken)
    {
        await AddActionLogAsync(
            userId,
            action,
            entityId: null,
            statusCode: 200,
            succeeded: true,
            failureReason: null,
            metadata: new
            {
                page,
                take,
                hasSearch = !string.IsNullOrWhiteSpace(request.Search),
                filterCount = request.Filters?.Length ?? 0,
                sortCount = request.Sorting?.Length ?? 0,
                groupBy = NormalizeGridField(request.GroupBy),
                selectedCount
            },
            cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task SaveFailureLogAsync(
        long userId,
        string action,
        string? entityId,
        int statusCode,
        string failureReason,
        CancellationToken cancellationToken)
    {
        await AddActionLogAsync(
            userId,
            action,
            entityId,
            statusCode,
            succeeded: false,
            failureReason,
            metadata: null,
            cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private Task AddActionLogAsync(
        long userId,
        string action,
        string? entityId,
        int statusCode,
        bool succeeded,
        string? failureReason,
        object? metadata,
        CancellationToken cancellationToken)
    {
        return _actionLogService.AddAsync(new ActionLogRequest
        {
            UserId = userId,
            Action = action,
            Module = "tenant",
            EntityName = "Tenant",
            EntityId = entityId,
            StatusCode = statusCode,
            Succeeded = succeeded,
            FailureReason = failureReason,
            MetadataJson = metadata is null ? null : JsonSerializer.Serialize(metadata)
        }, cancellationToken);
    }

    private sealed record TenantInput(
        string Code,
        string Name,
        string? Address,
        string Phone,
        long RegionId,
        long DistrictId);

    private sealed record TenantInputValidationResult(TenantInput? Value, string? Error)
    {
        public static TenantInputValidationResult Succeeded(TenantInput value) => new(value, null);
        public static TenantInputValidationResult Failed(string error) => new(null, error);
    }
}
