using Application.Audit;
using Application.Patients;
using Contracts.Patients;
using Domain.Patients;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.Json;
using ContractGender = Contracts.Patients.PatientGenderDto;
using DomainGender = Domain.Patients.PatientGender;
namespace Infrastructure.Patients;

public sealed class PatientService : IPatientService
{
    private const int DefaultGridPageSize = 25;
    private const int MaxGridPageSize = 100;
    private const int MaxExportRows = 50_000;
    private readonly AppDbContext _db;
    private readonly IActionLogService _actionLogService;

    public PatientService(AppDbContext db, IActionLogService actionLogService)
    {
        _db = db;
        _actionLogService = actionLogService;
    }

    public async Task<PatientGridQueryResult> QueryGridAsync(
        PatientGridQueryRequest request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(
            request.PageSize <= 0 ? DefaultGridPageSize : request.PageSize,
            1,
            MaxGridPageSize);
        var filteredQuery = ApplyGridRequest(
            ProjectToGridRow(GetPatientQuery()),
            request);
        var totalCount = await filteredQuery.CountAsync(cancellationToken);
        var groups = await LoadGroupSummariesAsync(
            filteredQuery,
            request.GroupBy,
            cancellationToken);
        var items = await ApplySorting(filteredQuery, request.Sorting, request.GroupBy)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        await AddGridActionLogAsync(
            "PatientGridQueried",
            request,
            page,
            pageSize,
            selectedCount: 0,
            cancellationToken);

        return new PatientGridQueryResult(items, totalCount, groups);
    }

    public async Task<PatientGridQueryResult> ExportGridAsync(
        PatientGridExportRequest request,
        CancellationToken cancellationToken)
    {
        var query = ApplyGridRequest(
            ProjectToGridRow(GetPatientQuery()),
            request);
        var selectedIds = request.SelectedIds
            .Distinct()
            .Take(MaxExportRows)
            .ToArray();

        if (selectedIds.Length > 0)
        {
            query = query.Where(x => selectedIds.Contains(x.Id));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var groups = await LoadGroupSummariesAsync(
            query,
            request.GroupBy,
            cancellationToken);
        var items = await ApplySorting(query, request.Sorting, request.GroupBy)
            .Take(MaxExportRows)
            .ToListAsync(cancellationToken);

        await AddGridActionLogAsync(
            "PatientGridExported",
            request,
            page: 1,
            take: items.Count,
            selectedIds.Length,
            cancellationToken);

        return new PatientGridQueryResult(items, totalCount, groups);
    }

    public async Task<IReadOnlyList<PatientGroupDto>> GetGroupsAsync(
        CancellationToken cancellationToken)
    {
        var groups = await _db.PatientGroups
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Id)
            .Select(x => new PatientGroupDto(x.Id, x.Code, x.Name))
            .ToListAsync(cancellationToken);

        await _actionLogService.AddAsync(new ActionLogRequest
        {
            Action = "PatientGroupsViewed",
            Module = "patient",
            EntityName = "PatientGroup",
            StatusCode = 200,
            Succeeded = true,
            MetadataJson = JsonSerializer.Serialize(new { resultCount = groups.Count })
        }, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return groups;
    }

    public async Task<IReadOnlyList<PatientDto>> GetAsync(
        string? search,
        long? groupId,
        CancellationToken cancellationToken)
    {
        var query = GetPatientQuery();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim().ToLower();
            query = query.Where(x =>
                x.Inn.Contains(value) ||
                x.FirstName.ToLower().Contains(value) ||
                x.LastName.ToLower().Contains(value) ||
                x.MiddleName.ToLower().Contains(value));
        }

        if (groupId is not null)
        {
            query = query.Where(x => x.GroupId == groupId);
        }

        query = query
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName);

        var patients = await ProjectToDto(query).ToListAsync(cancellationToken);

        await _actionLogService.AddAsync(new ActionLogRequest
        {
            Action = "PatientsViewed",
            Module = "patient",
            EntityName = "Patient",
            StatusCode = 200,
            Succeeded = true,
            MetadataJson = $$"""{"hasSearch":{{(!string.IsNullOrWhiteSpace(search)).ToString().ToLowerInvariant()}},"groupId":{{(groupId?.ToString() ?? "null")}},"resultCount":{{patients.Count}}}"""
        }, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return patients;
    }

    public async Task<PatientDto?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var query = _db.Patients
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted);

        var patient = await ProjectToDto(query).FirstOrDefaultAsync(cancellationToken);

        await _actionLogService.AddAsync(new ActionLogRequest
        {
            Action = patient is null ? "PatientViewFailed" : "PatientViewed",
            Module = "patient",
            EntityName = "Patient",
            EntityId = id.ToString(),
            StatusCode = patient is null ? 404 : 200,
            Succeeded = patient is not null,
            FailureReason = patient is null ? "Patient not found" : null
        }, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return patient;
    }

    public async Task<PatientDto?> GetByInnAsync(string inn, CancellationToken cancellationToken)
    {
        var query = _db.Patients
            .AsNoTracking()
            .Where(x => x.Inn == inn && !x.IsDeleted);

        var patient = await ProjectToDto(query).FirstOrDefaultAsync(cancellationToken);

        await _actionLogService.AddAsync(new ActionLogRequest
        {
            Action = patient is null ? "PatientLookupFailed" : "PatientLookupSucceeded",
            Module = "patient",
            EntityName = "Patient",
            EntityId = patient?.Id.ToString(),
            StatusCode = patient is null ? 404 : 200,
            Succeeded = patient is not null,
            FailureReason = patient is null ? "Patient not found by INN" : null,
            MetadataJson = $$"""{"lookup":"inn","found":{{(patient is not null).ToString().ToLowerInvariant()}}}"""
        }, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return patient;
    }

    public async Task<PatientCommandResult<PatientDto>> CreateAsync(
        long userId,
        CreatePatientRequest request,
        CancellationToken cancellationToken)
    {
        if (!await IsValidLocationAsync(request.RegionId, request.DistrictId, cancellationToken))
        {
            await _actionLogService.AddAsync(new ActionLogRequest
            {
                UserId = userId,
                Action = "PatientCreateFailed",
                Module = "patient",
                EntityName = "Patient",
                StatusCode = 400,
                Succeeded = false,
                FailureReason = "District does not belong to the selected region"
            }, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return new PatientCommandResult<PatientDto>(PatientCommandStatus.ValidationFailed);
        }

        var exists = await _db.Patients.AnyAsync(x =>
            x.Inn == request.Inn &&
            !x.IsDeleted,
            cancellationToken);

        if (exists)
        {
            await _actionLogService.AddAsync(new ActionLogRequest
            {
                UserId = userId,
                Action = "PatientCreateFailed",
                Module = "patient",
                EntityName = "Patient",
                StatusCode = 409,
                Succeeded = false,
                FailureReason = "Patient with same INN already exists"
            }, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return new PatientCommandResult<PatientDto>(PatientCommandStatus.Conflict);
        }

        var patient = new Patient
        {
            Inn = request.Inn,
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName,
            BirthDate = request.BirthDate!.Value,
            Gender = (DomainGender)request.Gender,
            Address = request.Address,
            Address2 = request.Address2,
            Phone = request.Phone,
            DistrictId = request.DistrictId,
            RegionId = request.RegionId,
            GroupId = PatientGroupIds.New,
            SpecialStatus = false,
            IsActive = true
        };

        _db.Patients.Add(patient);
        await _db.SaveChangesAsync(cancellationToken);

        await _actionLogService.AddAsync(new ActionLogRequest
        {
            UserId = userId,
            Action = "PatientCreated",
            Module = "patient",
            EntityName = "Patient",
            EntityId = patient.Id.ToString(),
            StatusCode = 201,
            Succeeded = true
        }, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        var createdPatient = await GetByIdAsync(patient.Id, cancellationToken);
        return new PatientCommandResult<PatientDto>(
            PatientCommandStatus.Succeeded,
            createdPatient);
    }

    public async Task<PatientCommandResult<PatientDto>> UpdateAsync(
        long id,
        long userId,
        UpdatePatientRequest request,
        CancellationToken cancellationToken)
    {
        var patient = await _db.Patients
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (patient is null)
        {
            await _actionLogService.AddAsync(new ActionLogRequest
            {
                UserId = userId,
                Action = "PatientUpdateFailed",
                Module = "patient",
                EntityName = "Patient",
                EntityId = id.ToString(),
                StatusCode = 404,
                Succeeded = false,
                FailureReason = "Patient not found"
            }, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return new PatientCommandResult<PatientDto>(PatientCommandStatus.NotFound);
        }

        if (!await IsValidLocationAsync(request.RegionId, request.DistrictId, cancellationToken))
        {
            await _actionLogService.AddAsync(new ActionLogRequest
            {
                UserId = userId,
                Action = "PatientUpdateFailed",
                Module = "patient",
                EntityName = "Patient",
                EntityId = id.ToString(),
                StatusCode = 400,
                Succeeded = false,
                FailureReason = "District does not belong to the selected region"
            }, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return new PatientCommandResult<PatientDto>(PatientCommandStatus.ValidationFailed);
        }

        var duplicateInn = await _db.Patients.AnyAsync(x =>
            x.Id != id &&
            x.Inn == request.Inn &&
            !x.IsDeleted,
            cancellationToken);

        if (duplicateInn)
        {
            await _actionLogService.AddAsync(new ActionLogRequest
            {
                UserId = userId,
                Action = "PatientUpdateFailed",
                Module = "patient",
                EntityName = "Patient",
                EntityId = id.ToString(),
                StatusCode = 409,
                Succeeded = false,
                FailureReason = "Patient with same INN already exists"
            }, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return new PatientCommandResult<PatientDto>(PatientCommandStatus.Conflict);
        }

        var groupExists = await _db.PatientGroups.AnyAsync(x =>
            x.Id == request.GroupId &&
            x.IsActive,
            cancellationToken);

        if (!groupExists)
        {
            await _actionLogService.AddAsync(new ActionLogRequest
            {
                UserId = userId,
                Action = "PatientUpdateFailed",
                Module = "patient",
                EntityName = "Patient",
                EntityId = id.ToString(),
                StatusCode = 409,
                Succeeded = false,
                FailureReason = "Patient group not found or inactive",
                MetadataJson = $$"""{"groupId":{{request.GroupId}}}"""
            }, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return new PatientCommandResult<PatientDto>(PatientCommandStatus.ValidationFailed);
        }

        patient.Inn = request.Inn;
        patient.FirstName = request.FirstName;
        patient.LastName = request.LastName;
        patient.MiddleName = request.MiddleName;
        patient.BirthDate = request.BirthDate!.Value;
        patient.Gender = (DomainGender)request.Gender;
        patient.Address = request.Address;
        patient.Address2 = request.Address2;
        patient.Phone = request.Phone;
        patient.DistrictId = request.DistrictId;
        patient.RegionId = request.RegionId;
        patient.GroupId = request.GroupId;
        patient.SpecialStatus = request.SpecialStatus;
        patient.IsActive = request.IsActive;

        await _actionLogService.AddAsync(new ActionLogRequest
        {
            UserId = userId,
            Action = "PatientUpdated",
            Module = "patient",
            EntityName = "Patient",
            EntityId = patient.Id.ToString(),
            StatusCode = 200,
            Succeeded = true
        }, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        var updatedPatient = await GetByIdAsync(patient.Id, cancellationToken);
        return new PatientCommandResult<PatientDto>(
            PatientCommandStatus.Succeeded,
            updatedPatient);
    }

    public async Task<bool> DeleteAsync(long id, long userId, CancellationToken cancellationToken)
    {
        var patient = await _db.Patients
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (patient is null)
        {
            await _actionLogService.AddAsync(new ActionLogRequest
            {
                UserId = userId,
                Action = "PatientDeleteFailed",
                Module = "patient",
                EntityName = "Patient",
                EntityId = id.ToString(),
                StatusCode = 404,
                Succeeded = false,
                FailureReason = "Patient not found"
            }, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return false;
        }

        patient.IsDeleted = true;

        await _actionLogService.AddAsync(new ActionLogRequest
        {
            UserId = userId,
            Action = "PatientDeleted",
            Module = "patient",
            EntityName = "Patient",
            EntityId = patient.Id.ToString(),
            StatusCode = 204,
            Succeeded = true
        }, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static IQueryable<PatientDto> ProjectToDto(IQueryable<Patient> query)
    {
        return query.Select(patient => new PatientDto(
            patient.Id,
            patient.Inn,
            patient.FirstName,
            patient.LastName,
            patient.MiddleName,
            patient.BirthDate,
            (ContractGender)patient.Gender,
            patient.Address,
            patient.Address2,
            patient.Phone,
            patient.DistrictId,
            patient.RegionId,
            patient.GroupId,
            patient.Group.Code,
            patient.Group.Name,
            patient.SpecialStatus,
            patient.CreatedAt,
            patient.UpdatedAt,
            patient.IsActive));
    }

    private Task<bool> IsValidLocationAsync(
        long regionId,
        long districtId,
        CancellationToken cancellationToken)
    {
        return _db.Districts
            .AsNoTracking()
            .AnyAsync(
                district => district.Id == districtId
                    && district.RegionId == regionId
                    && district.IsActive
                    && !district.IsDeleted
                    && district.Region.IsActive
                    && !district.Region.IsDeleted,
                cancellationToken);
    }

    private IQueryable<Patient> GetPatientQuery()
    {
        return _db.Patients
            .AsNoTracking()
            .Where(x => !x.IsDeleted);
    }

    private static IQueryable<PatientGridRowDto> ProjectToGridRow(IQueryable<Patient> query)
    {
        return query.Select(patient => new PatientGridRowDto
        {
            Id = patient.Id,
            Inn = patient.Inn,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            MiddleName = patient.MiddleName,
            FullName = patient.LastName + " " + patient.FirstName + " " + patient.MiddleName,
            BirthDate = patient.BirthDate,
            Gender = (ContractGender)patient.Gender,
            Address = patient.Address,
            Address2 = patient.Address2,
            Phone = patient.Phone,
            RegionId = patient.RegionId,
            RegionName = patient.Region.Name,
            DistrictId = patient.DistrictId,
            DistrictName = patient.District.Name,
            GroupId = patient.GroupId,
            GroupCode = patient.Group.Code,
            GroupName = patient.Group.Name,
            SpecialStatus = patient.SpecialStatus,
            CreatedAt = patient.CreatedAt,
            UpdatedAt = patient.UpdatedAt,
            IsActive = patient.IsActive
        });
    }

    private static IQueryable<PatientGridRowDto> ApplyGridRequest(
        IQueryable<PatientGridRowDto> query,
        PatientGridQueryRequest request)
    {
        var search = request.Search?.Trim().ToLowerInvariant();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(patient =>
                patient.Inn.ToLower().Contains(search) ||
                patient.FullName.ToLower().Contains(search) ||
                patient.Phone.ToLower().Contains(search) ||
                patient.RegionName.ToLower().Contains(search) ||
                patient.DistrictName.ToLower().Contains(search));
        }

        foreach (var filter in (request.Filters ?? []).Take(20))
        {
            query = ApplyFilter(query, filter);
        }

        return query;
    }

    private static IQueryable<PatientGridRowDto> ApplyFilter(
        IQueryable<PatientGridRowDto> query,
        PatientGridFilterDto filter)
    {
        return NormalizeGridField(filter.Field) switch
        {
            "id" => ApplyLongFilter(query, filter, patient => patient.Id),
            "inn" => ApplyStringFilter(query, filter, patient => patient.Inn),
            "firstName" => ApplyStringFilter(query, filter, patient => patient.FirstName),
            "lastName" => ApplyStringFilter(query, filter, patient => patient.LastName),
            "middleName" => ApplyStringFilter(query, filter, patient => patient.MiddleName),
            "fullName" => ApplyStringFilter(query, filter, patient => patient.FullName),
            "phone" => ApplyStringFilter(query, filter, patient => patient.Phone),
            "address" => ApplyStringFilter(query, filter, patient => patient.Address),
            "address2" => ApplyStringFilter(query, filter, patient => patient.Address2),
            "regionId" => ApplyLongFilter(query, filter, patient => patient.RegionId),
            "regionName" => ApplyStringFilter(query, filter, patient => patient.RegionName),
            "districtId" => ApplyLongFilter(query, filter, patient => patient.DistrictId),
            "districtName" => ApplyStringFilter(query, filter, patient => patient.DistrictName),
            "groupId" => ApplyLongFilter(query, filter, patient => patient.GroupId),
            "groupName" => ApplyStringFilter(query, filter, patient => patient.GroupName),
            "gender" => ApplyGenderFilter(query, filter),
            "specialStatus" => ApplyBooleanFilter(query, filter, patient => patient.SpecialStatus),
            "isActive" => ApplyBooleanFilter(query, filter, patient => patient.IsActive),
            "birthDate" => ApplyDateOnlyFilter(query, filter),
            "createdAt" => ApplyDateTimeFilter(query, filter, useUpdatedAt: false),
            "updatedAt" => ApplyDateTimeFilter(query, filter, useUpdatedAt: true),
            _ => query
        };
    }

    private static IQueryable<PatientGridRowDto> ApplyStringFilter(
        IQueryable<PatientGridRowDto> query,
        PatientGridFilterDto filter,
        Expression<Func<PatientGridRowDto, string>> selector)
    {
        var operation = NormalizeOperator(filter.Operator);
        var value = filter.Value?.Trim().ToLowerInvariant() ?? string.Empty;
        var loweredSelector = Expression.Call(
            selector.Body,
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
            "isEmpty" => Expression.Equal(selector.Body, Expression.Constant(string.Empty)),
            "isNotEmpty" => Expression.NotEqual(selector.Body, Expression.Constant(string.Empty)),
            _ => null
        };

        if (predicate is null)
        {
            return query;
        }

        return query.Where(Expression.Lambda<Func<PatientGridRowDto, bool>>(
            predicate,
            selector.Parameters));
    }

    private static IQueryable<PatientGridRowDto> ApplyLongFilter(
        IQueryable<PatientGridRowDto> query,
        PatientGridFilterDto filter,
        Expression<Func<PatientGridRowDto, long>> selector)
    {
        if (!long.TryParse(filter.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            return query;
        }

        var constant = Expression.Constant(value);
        Expression? predicate = NormalizeOperator(filter.Operator) switch
        {
            "equals" => Expression.Equal(selector.Body, constant),
            "notEquals" => Expression.NotEqual(selector.Body, constant),
            "greaterThan" => Expression.GreaterThan(selector.Body, constant),
            "greaterThanOrEqual" => Expression.GreaterThanOrEqual(selector.Body, constant),
            "lessThan" => Expression.LessThan(selector.Body, constant),
            "lessThanOrEqual" => Expression.LessThanOrEqual(selector.Body, constant),
            _ => null
        };

        if (predicate is null)
        {
            return query;
        }

        return query.Where(Expression.Lambda<Func<PatientGridRowDto, bool>>(
            predicate,
            selector.Parameters));
    }

    private static IQueryable<PatientGridRowDto> ApplyGenderFilter(
        IQueryable<PatientGridRowDto> query,
        PatientGridFilterDto filter)
    {
        return int.TryParse(filter.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            && Enum.IsDefined(typeof(ContractGender), value)
            ? NormalizeOperator(filter.Operator) == "notEquals"
                ? query.Where(patient => (int)patient.Gender != value)
                : query.Where(patient => (int)patient.Gender == value)
            : query;
    }

    private static IQueryable<PatientGridRowDto> ApplyBooleanFilter(
        IQueryable<PatientGridRowDto> query,
        PatientGridFilterDto filter,
        Expression<Func<PatientGridRowDto, bool>> selector)
    {
        if (!bool.TryParse(filter.Value, out var value))
        {
            return query;
        }

        var predicate = NormalizeOperator(filter.Operator) == "notEquals"
            ? Expression.NotEqual(selector.Body, Expression.Constant(value))
            : Expression.Equal(selector.Body, Expression.Constant(value));

        return query.Where(Expression.Lambda<Func<PatientGridRowDto, bool>>(
            predicate,
            selector.Parameters));
    }

    private static IQueryable<PatientGridRowDto> ApplyDateOnlyFilter(
        IQueryable<PatientGridRowDto> query,
        PatientGridFilterDto filter)
    {
        if (!TryParseDate(filter.Value, out var value))
        {
            return query;
        }

        return NormalizeOperator(filter.Operator) switch
        {
            "equals" => query.Where(patient => patient.BirthDate == value),
            "notEquals" => query.Where(patient => patient.BirthDate != value),
            "greaterThan" => query.Where(patient => patient.BirthDate > value),
            "greaterThanOrEqual" => query.Where(patient => patient.BirthDate >= value),
            "lessThan" => query.Where(patient => patient.BirthDate < value),
            "lessThanOrEqual" => query.Where(patient => patient.BirthDate <= value),
            "between" when TryParseDate(filter.ValueTo, out var valueTo) =>
                query.Where(patient => patient.BirthDate >= value && patient.BirthDate <= valueTo),
            _ => query
        };
    }

    private static IQueryable<PatientGridRowDto> ApplyDateTimeFilter(
        IQueryable<PatientGridRowDto> query,
        PatientGridFilterDto filter,
        bool useUpdatedAt)
    {
        if (!TryParseDate(filter.Value, out var date))
        {
            return query;
        }

        var start = new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var end = start.AddDays(1);
        var operation = NormalizeOperator(filter.Operator);

        if (useUpdatedAt)
        {
            return operation switch
            {
                "equals" => query.Where(patient => patient.UpdatedAt >= start && patient.UpdatedAt < end),
                "notEquals" => query.Where(patient => patient.UpdatedAt < start || patient.UpdatedAt >= end),
                "greaterThan" => query.Where(patient => patient.UpdatedAt >= end),
                "greaterThanOrEqual" => query.Where(patient => patient.UpdatedAt >= start),
                "lessThan" => query.Where(patient => patient.UpdatedAt < start),
                "lessThanOrEqual" => query.Where(patient => patient.UpdatedAt < end),
                "between" when TryParseDate(filter.ValueTo, out var dateTo) =>
                    query.Where(patient => patient.UpdatedAt >= start && patient.UpdatedAt < new DateTimeOffset(dateTo.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)),
                "isEmpty" => query.Where(patient => patient.UpdatedAt == null),
                "isNotEmpty" => query.Where(patient => patient.UpdatedAt != null),
                _ => query
            };
        }

        return operation switch
        {
            "equals" => query.Where(patient => patient.CreatedAt >= start && patient.CreatedAt < end),
            "notEquals" => query.Where(patient => patient.CreatedAt < start || patient.CreatedAt >= end),
            "greaterThan" => query.Where(patient => patient.CreatedAt >= end),
            "greaterThanOrEqual" => query.Where(patient => patient.CreatedAt >= start),
            "lessThan" => query.Where(patient => patient.CreatedAt < start),
            "lessThanOrEqual" => query.Where(patient => patient.CreatedAt < end),
            "between" when TryParseDate(filter.ValueTo, out var dateTo) =>
                query.Where(patient => patient.CreatedAt >= start && patient.CreatedAt < new DateTimeOffset(dateTo.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)),
            _ => query
        };
    }

    private static IQueryable<PatientGridRowDto> ApplySorting(
        IQueryable<PatientGridRowDto> query,
        IReadOnlyList<PatientGridSortDto>? sorting,
        string? groupBy)
    {
        IOrderedQueryable<PatientGridRowDto>? ordered = null;
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
            return query
                .OrderByDescending(patient => patient.CreatedAt)
                .ThenByDescending(patient => patient.Id);
        }

        if (!(sorting ?? []).Any(sort => NormalizeGridField(sort.Field) == "id"))
        {
            ordered = ordered.ThenByDescending(patient => patient.Id);
        }

        return ordered;
    }

    private static IOrderedQueryable<PatientGridRowDto>? ApplySortField(
        IQueryable<PatientGridRowDto> query,
        IOrderedQueryable<PatientGridRowDto>? ordered,
        string? field,
        bool descending)
    {
        return field switch
        {
            "id" => ApplyOrder(query, ordered, patient => patient.Id, descending),
            "inn" => ApplyOrder(query, ordered, patient => patient.Inn, descending),
            "firstName" => ApplyOrder(query, ordered, patient => patient.FirstName, descending),
            "lastName" => ApplyOrder(query, ordered, patient => patient.LastName, descending),
            "middleName" => ApplyOrder(query, ordered, patient => patient.MiddleName, descending),
            "fullName" => ApplyOrder(query, ordered, patient => patient.FullName, descending),
            "birthDate" => ApplyOrder(query, ordered, patient => patient.BirthDate, descending),
            "gender" => ApplyOrder(query, ordered, patient => patient.Gender, descending),
            "phone" => ApplyOrder(query, ordered, patient => patient.Phone, descending),
            "regionName" => ApplyOrder(query, ordered, patient => patient.RegionName, descending),
            "districtName" => ApplyOrder(query, ordered, patient => patient.DistrictName, descending),
            "groupName" => ApplyOrder(query, ordered, patient => patient.GroupName, descending),
            "specialStatus" => ApplyOrder(query, ordered, patient => patient.SpecialStatus, descending),
            "isActive" => ApplyOrder(query, ordered, patient => patient.IsActive, descending),
            "createdAt" => ApplyOrder(query, ordered, patient => patient.CreatedAt, descending),
            "updatedAt" => ApplyOrder(query, ordered, patient => patient.UpdatedAt, descending),
            _ => null
        };
    }

    private static IOrderedQueryable<PatientGridRowDto> ApplyOrder<TKey>(
        IQueryable<PatientGridRowDto> query,
        IOrderedQueryable<PatientGridRowDto>? ordered,
        Expression<Func<PatientGridRowDto, TKey>> selector,
        bool descending)
    {
        if (ordered is null)
        {
            return descending
                ? query.OrderByDescending(selector)
                : query.OrderBy(selector);
        }

        return descending
            ? ordered.ThenByDescending(selector)
            : ordered.ThenBy(selector);
    }

    private static async Task<IReadOnlyList<PatientGridGroupSummaryDto>> LoadGroupSummariesAsync(
        IQueryable<PatientGridRowDto> query,
        string? groupBy,
        CancellationToken cancellationToken)
    {
        switch (NormalizeGridField(groupBy))
        {
            case "regionName":
            {
                var groups = await query
                    .GroupBy(patient => patient.RegionName)
                    .Select(group => new { Key = group.Key, Count = group.Count() })
                    .OrderBy(group => group.Key)
                    .ToListAsync(cancellationToken);
                return groups
                    .Select(group => new PatientGridGroupSummaryDto(group.Key, group.Key, group.Count))
                    .ToArray();
            }
            case "districtName":
            {
                var groups = await query
                    .GroupBy(patient => patient.DistrictName)
                    .Select(group => new { Key = group.Key, Count = group.Count() })
                    .OrderBy(group => group.Key)
                    .ToListAsync(cancellationToken);
                return groups
                    .Select(group => new PatientGridGroupSummaryDto(group.Key, group.Key, group.Count))
                    .ToArray();
            }
            case "groupName":
            {
                var groups = await query
                    .GroupBy(patient => patient.GroupName)
                    .Select(group => new { Key = group.Key, Count = group.Count() })
                    .OrderBy(group => group.Key)
                    .ToListAsync(cancellationToken);
                return groups
                    .Select(group => new PatientGridGroupSummaryDto(group.Key, group.Key, group.Count))
                    .ToArray();
            }
            case "gender":
            {
                var groups = await query
                    .GroupBy(patient => patient.Gender)
                    .Select(group => new { Key = group.Key, Count = group.Count() })
                    .OrderBy(group => group.Key)
                    .ToListAsync(cancellationToken);
                return groups
                    .Select(group => new PatientGridGroupSummaryDto(
                        ((int)group.Key).ToString(CultureInfo.InvariantCulture),
                        group.Key == ContractGender.Male ? "Male" : "Female",
                        group.Count))
                    .ToArray();
            }
            case "specialStatus":
            {
                var groups = await query
                    .GroupBy(patient => patient.SpecialStatus)
                    .Select(group => new { Key = group.Key, Count = group.Count() })
                    .OrderByDescending(group => group.Key)
                    .ToListAsync(cancellationToken);
                return groups
                    .Select(group => new PatientGridGroupSummaryDto(
                        group.Key.ToString().ToLowerInvariant(),
                        group.Key ? "Special" : "Standard",
                        group.Count))
                    .ToArray();
            }
            case "isActive":
            {
                var groups = await query
                    .GroupBy(patient => patient.IsActive)
                    .Select(group => new { Key = group.Key, Count = group.Count() })
                    .OrderByDescending(group => group.Key)
                    .ToListAsync(cancellationToken);
                return groups
                    .Select(group => new PatientGridGroupSummaryDto(
                        group.Key.ToString().ToLowerInvariant(),
                        group.Key ? "Active" : "Inactive",
                        group.Count))
                    .ToArray();
            }
            default:
                return [];
        }
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

    private static string? NormalizeGridField(string? field)
    {
        return field?.Trim().ToLowerInvariant() switch
        {
            "id" => "id",
            "inn" => "inn",
            "firstname" => "firstName",
            "lastname" => "lastName",
            "middlename" => "middleName",
            "fullname" => "fullName",
            "birthdate" => "birthDate",
            "gender" => "gender",
            "phone" => "phone",
            "address" => "address",
            "address2" => "address2",
            "regionid" => "regionId",
            "regionname" => "regionName",
            "districtid" => "districtId",
            "districtname" => "districtName",
            "groupid" => "groupId",
            "groupname" => "groupName",
            "specialstatus" => "specialStatus",
            "isactive" => "isActive",
            "createdat" => "createdAt",
            "updatedat" => "updatedAt",
            _ => null
        };
    }

    private static bool IsGroupField(string? field)
    {
        return field is "regionName"
            or "districtName"
            or "groupName"
            or "gender"
            or "specialStatus"
            or "isActive";
    }

    private async Task AddGridActionLogAsync(
        string action,
        PatientGridQueryRequest request,
        int page,
        int take,
        int selectedCount,
        CancellationToken cancellationToken)
    {
        await _actionLogService.AddAsync(new ActionLogRequest
        {
            Action = action,
            Module = "patient",
            EntityName = "Patient",
            StatusCode = 200,
            Succeeded = true,
            MetadataJson = JsonSerializer.Serialize(new
            {
                page,
                take,
                hasSearch = !string.IsNullOrWhiteSpace(request.Search),
                filterCount = request.Filters?.Length ?? 0,
                sortCount = request.Sorting?.Length ?? 0,
                groupBy = NormalizeGridField(request.GroupBy),
                selectedCount
            })
        }, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
    }
}
