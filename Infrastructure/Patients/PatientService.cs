using Application.Audit;
using Application.Patients;
using Contracts.Patients;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Data.Helpers;
using DevExtreme.AspNet.Data.ResponseModel;
using Domain.Patients;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
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

    public async Task<PatientGridLoadResult> LoadGridAsync(
        PatientGridLoadRequest request,
        CancellationToken cancellationToken)
    {
        var options = CreateLoadOptions(request, isExport: false);
        var result = await DataSourceLoader.LoadAsync(
            ProjectToGridRow(GetPatientQuery()),
            options,
            cancellationToken);

        await AddGridActionLogAsync(
            "PatientGridQueried",
            request,
            options.Take,
            selectedCount: 0,
            cancellationToken);

        return ToGridLoadResult(result);
    }

    public async Task<PatientGridLoadResult> ExportGridAsync(
        PatientGridExportRequest request,
        CancellationToken cancellationToken)
    {
        var query = ProjectToGridRow(GetPatientQuery());
        var selectedIds = request.SelectedIds
            .Distinct()
            .Take(MaxExportRows)
            .ToArray();

        if (selectedIds.Length > 0)
        {
            query = query.Where(x => selectedIds.Contains(x.Id));
        }

        var options = CreateLoadOptions(request, isExport: true);
        var result = await DataSourceLoader.LoadAsync(query, options, cancellationToken);

        await AddGridActionLogAsync(
            "PatientGridExported",
            request,
            options.Take,
            selectedIds.Length,
            cancellationToken);

        return ToGridLoadResult(result);
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
            SpecialStatus = request.SpecialStatus,
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

    private static DataSourceLoadOptionsBase CreateLoadOptions(
        PatientGridLoadRequest request,
        bool isExport)
    {
        var options = new DataSourceLoadOptionsBase();
        DataSourceLoadOptionsParser.Parse(
            options,
            optionName => GetLoadOptionValue(request, optionName));

        options.PrimaryKey = [nameof(PatientGridRowDto.Id)];
        options.DefaultSort = nameof(PatientGridRowDto.Id);
        options.StringToLower = true;
        options.RequireTotalCount = true;

        if (isExport)
        {
            options.Skip = 0;
            options.Take = MaxExportRows;
        }
        else
        {
            options.Skip = Math.Max(0, options.Skip);
            options.Take = Math.Clamp(
                options.Take <= 0 ? DefaultGridPageSize : options.Take,
                1,
                MaxGridPageSize);
        }

        return options;
    }

    private static string GetLoadOptionValue(
        PatientGridLoadRequest request,
        string optionName)
    {
        return optionName switch
        {
            "skip" => request.Skip.ToString(CultureInfo.InvariantCulture),
            "take" => request.Take.ToString(CultureInfo.InvariantCulture),
            "requireTotalCount" => request.RequireTotalCount.ToString(CultureInfo.InvariantCulture),
            "requireGroupCount" => request.RequireGroupCount.ToString(CultureInfo.InvariantCulture),
            "isCountQuery" => request.IsCountQuery.ToString(CultureInfo.InvariantCulture),
            "sort" => request.Sort ?? string.Empty,
            "group" => request.Group ?? string.Empty,
            "filter" => request.Filter ?? string.Empty,
            "totalSummary" => request.TotalSummary ?? string.Empty,
            "groupSummary" => request.GroupSummary ?? string.Empty,
            _ => string.Empty
        };
    }

    private static PatientGridLoadResult ToGridLoadResult(LoadResult result)
    {
        return new PatientGridLoadResult(
            result.data,
            result.totalCount,
            result.groupCount,
            result.summary);
    }

    private async Task AddGridActionLogAsync(
        string action,
        PatientGridLoadRequest request,
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
                skip = Math.Max(0, request.Skip),
                take,
                hasFilter = !string.IsNullOrWhiteSpace(request.Filter),
                hasSorting = !string.IsNullOrWhiteSpace(request.Sort),
                hasGrouping = !string.IsNullOrWhiteSpace(request.Group),
                selectedCount
            })
        }, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
    }
}
