using Application.Audit;
using Application.Patients;
using Contracts.Patients;
using Domain.Patients;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ContractGender = Contracts.Patients.PatientGenderDto;
using DomainGender = Domain.Patients.PatientGender;
namespace Infrastructure.Patients;

public sealed class PatientService : IPatientService
{
    private readonly AppDbContext _db;
    private readonly IActionLogService _actionLogService;

    public PatientService(AppDbContext db, IActionLogService actionLogService)
    {
        _db = db;
        _actionLogService = actionLogService;
    }

    public async Task<IReadOnlyList<PatientDto>> GetAsync(
        string? search,
        long? groupId,
        CancellationToken cancellationToken)
    {
        var query = _db.Patients
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

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

    public async Task<PatientDto?> CreateAsync(
        long userId,
        CreatePatientRequest request,
        CancellationToken cancellationToken)
    {
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
                FailureReason = "Patient with same INN already exists",
                MetadataJson = $$"""{"inn":"{{request.Inn}}"}"""
            }, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return null;
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
            Succeeded = true,
            MetadataJson = $$"""{"inn":"{{patient.Inn}}"}"""
        }, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(patient.Id, cancellationToken);
    }

    public async Task<PatientDto?> UpdateAsync(
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
            return null;
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
            return null;
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
            return null;
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
            Succeeded = true,
            MetadataJson = $$"""{"inn":"{{patient.Inn}}"}"""
        }, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(patient.Id, cancellationToken);
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
            Succeeded = true,
            MetadataJson = $$"""{"inn":"{{patient.Inn}}"}"""
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
}
