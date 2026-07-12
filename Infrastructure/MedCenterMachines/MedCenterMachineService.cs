using Application.Audit;
using Application.MedCenterMachines;
using Application.Tenants;
using Contracts.MedCenterMachines;
using Domain.MedCenters;
using Domain.Sessions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ContractType = Contracts.MedCenterMachines.MachineAcquisitionTypeDto;
using DomainType = Domain.MedCenters.MachineAcquisitionType;

namespace Infrastructure.MedCenterMachines;

public sealed class MedCenterMachineService : IMedCenterMachineService
{
    private readonly AppDbContext _db;
    private readonly IActionLogService _actionLogService;
    private readonly ITenantAccessService _tenantAccessService;

    public MedCenterMachineService(
        AppDbContext db,
        IActionLogService actionLogService,
        ITenantAccessService tenantAccessService)
    {
        _db = db;
        _actionLogService = actionLogService;
        _tenantAccessService = tenantAccessService;
    }

    public async Task<IReadOnlyList<MedCenterMachineDto>?> GetAsync(
        long userId,
        IReadOnlyList<string>? tenantIds,
        CancellationToken cancellationToken)
    {
        var filter = await _tenantAccessService.ResolveTenantFilterAsync(userId, tenantIds, cancellationToken);
        if (!filter.Succeeded)
        {
            return null;
        }

        return await _db.MedCenterMachines
            .AsNoTracking()
            .Where(x => filter.TenantIds.Contains(x.TenantId) && !x.IsDeleted)
            .OrderBy(x => x.TenantId)
            .ThenBy(x => x.InventoryNumber)
            .Select(x => new MedCenterMachineDto(
                x.Id,
                x.TenantId,
                (ContractType)x.AcquisitionType,
                x.InventoryNumber,
                x.Name,
                x.Model,
                x.SerialNumber,
                x.Manufacturer,
                x.ManufacturingCountry,
                x.ManufactureYear,
                x.CertificateHolder,
                x.CertificateHolderCountry,
                x.CertificateNumber,
                x.CertificateCountry,
                x.CertificateIssuedAt,
                x.PermitName,
                x.PermitNumber,
                x.PermitSeries,
                x.PermitExpiresAt,
                x.DailySessionLimit,
                x.BetweenSessionCooldownMinutes,
                x.DailyLimitCooldownMinutes,
                x.IsApproved,
                x.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<MedCenterMachineDto?> GetByIdAsync(
        long userId,
        string tenantId,
        long id,
        CancellationToken cancellationToken)
    {
        if (!await _tenantAccessService.CanAccessTenantAsync(userId, tenantId, cancellationToken))
        {
            return null;
        }

        return await _db.MedCenterMachines
            .AsNoTracking()
            .Where(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted)
            .Select(x => new MedCenterMachineDto(
                x.Id,
                x.TenantId,
                (ContractType)x.AcquisitionType,
                x.InventoryNumber,
                x.Name,
                x.Model,
                x.SerialNumber,
                x.Manufacturer,
                x.ManufacturingCountry,
                x.ManufactureYear,
                x.CertificateHolder,
                x.CertificateHolderCountry,
                x.CertificateNumber,
                x.CertificateCountry,
                x.CertificateIssuedAt,
                x.PermitName,
                x.PermitNumber,
                x.PermitSeries,
                x.PermitExpiresAt,
                x.DailySessionLimit,
                x.BetweenSessionCooldownMinutes,
                x.DailyLimitCooldownMinutes,
                x.IsApproved,
                x.IsActive))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<MedCenterMachineCommandResult<MedCenterMachineDto>> CreateAsync(
        string tenantId,
        long userId,
        CreateMedCenterMachineRequest request,
        CancellationToken cancellationToken)
    {
        if (!await _tenantAccessService.CanAccessTenantAsync(userId, tenantId, cancellationToken))
        {
            return new MedCenterMachineCommandResult<MedCenterMachineDto>(MedCenterMachineCommandStatus.Forbidden);
        }

        var duplicate = await HasDuplicateAsync(
            tenantId,
            null,
            request.InventoryNumber,
            request.SerialNumber,
            cancellationToken);

        if (duplicate)
        {
            await AddMachineLogAsync(userId, "MachineCreateFailed", null, 409, false, "Duplicate inventory number or serial number", tenantId, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return new MedCenterMachineCommandResult<MedCenterMachineDto>(MedCenterMachineCommandStatus.Conflict);
        }

        var machine = new MedCenterMachine
        {
            TenantId = tenantId
        };

        Apply(machine, request);

        _db.MedCenterMachines.Add(machine);
        await _db.SaveChangesAsync(cancellationToken);

        await AddMachineLogAsync(userId, "MachineCreated", machine.Id.ToString(), 201, true, null, tenantId, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return new MedCenterMachineCommandResult<MedCenterMachineDto>(
            MedCenterMachineCommandStatus.Succeeded,
            ToDto(machine));
    }

    public async Task<MedCenterMachineCommandResult<MedCenterMachineDto>> UpdateAsync(
        string tenantId,
        long id,
        long userId,
        UpdateMedCenterMachineRequest request,
        CancellationToken cancellationToken)
    {
        if (!await _tenantAccessService.CanAccessTenantAsync(userId, tenantId, cancellationToken))
        {
            return new MedCenterMachineCommandResult<MedCenterMachineDto>(MedCenterMachineCommandStatus.Forbidden);
        }

        var machine = await _db.MedCenterMachines
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, cancellationToken);

        if (machine is null)
        {
            return new MedCenterMachineCommandResult<MedCenterMachineDto>(MedCenterMachineCommandStatus.NotFound);
        }

        var duplicate = await HasDuplicateAsync(
            tenantId,
            id,
            request.InventoryNumber,
            request.SerialNumber,
            cancellationToken);

        if (duplicate)
        {
            await AddMachineLogAsync(userId, "MachineUpdateFailed", id.ToString(), 409, false, "Duplicate inventory number or serial number", tenantId, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return new MedCenterMachineCommandResult<MedCenterMachineDto>(MedCenterMachineCommandStatus.Conflict);
        }

        Apply(machine, request);

        await AddMachineLogAsync(userId, "MachineUpdated", machine.Id.ToString(), 200, true, null, tenantId, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return new MedCenterMachineCommandResult<MedCenterMachineDto>(
            MedCenterMachineCommandStatus.Succeeded,
            ToDto(machine));
    }

    public async Task<MedCenterMachineCommandResult<bool>> DeleteAsync(
        string tenantId,
        long id,
        long userId,
        CancellationToken cancellationToken)
    {
        if (!await _tenantAccessService.CanAccessTenantAsync(userId, tenantId, cancellationToken))
        {
            return new MedCenterMachineCommandResult<bool>(MedCenterMachineCommandStatus.Forbidden);
        }

        var machine = await _db.MedCenterMachines
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, cancellationToken);

        if (machine is null)
        {
            return new MedCenterMachineCommandResult<bool>(MedCenterMachineCommandStatus.NotFound);
        }

        var hasActiveSession = await _db.HdSessions.AnyAsync(x =>
            x.TenantId == tenantId &&
            x.MachineId == id &&
            (x.Status == SessionStatus.Started || x.Status == SessionStatus.Paused),
            cancellationToken);

        if (hasActiveSession)
        {
            await AddMachineLogAsync(userId, "MachineDeleteFailed", id.ToString(), 409, false, "Machine has active session", tenantId, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return new MedCenterMachineCommandResult<bool>(MedCenterMachineCommandStatus.Conflict);
        }

        machine.IsDeleted = true;
        machine.IsActive = false;

        await AddMachineLogAsync(userId, "MachineDeleted", machine.Id.ToString(), 204, true, null, tenantId, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return new MedCenterMachineCommandResult<bool>(MedCenterMachineCommandStatus.Succeeded, true);
    }

    private async Task<bool> HasDuplicateAsync(
        string tenantId,
        long? currentId,
        string inventoryNumber,
        string serialNumber,
        CancellationToken cancellationToken)
    {
        var normalizedInventoryNumber = inventoryNumber.Trim();
        var normalizedSerialNumber = serialNumber.Trim();

        return await _db.MedCenterMachines.AnyAsync(x =>
            x.Id != currentId &&
            !x.IsDeleted &&
            (
                (x.TenantId == tenantId && x.InventoryNumber == normalizedInventoryNumber) ||
                x.SerialNumber == normalizedSerialNumber
            ),
            cancellationToken);
    }

    private static void Apply(MedCenterMachine machine, CreateMedCenterMachineRequest request)
    {
        machine.AcquisitionType = (DomainType)request.AcquisitionType;
        machine.InventoryNumber = request.InventoryNumber.Trim();
        machine.Name = request.Name.Trim();
        machine.Model = request.Model.Trim();
        machine.SerialNumber = request.SerialNumber.Trim();
        machine.Manufacturer = request.Manufacturer.Trim();
        machine.ManufacturingCountry = Normalize(request.ManufacturingCountry);
        machine.ManufactureYear = request.ManufactureYear;
        machine.CertificateHolder = Normalize(request.CertificateHolder);
        machine.CertificateHolderCountry = Normalize(request.CertificateHolderCountry);
        machine.CertificateNumber = Normalize(request.CertificateNumber);
        machine.CertificateCountry = Normalize(request.CertificateCountry);
        machine.CertificateIssuedAt = request.CertificateIssuedAt!.Value;
        machine.PermitName = Normalize(request.PermitName);
        machine.PermitNumber = Normalize(request.PermitNumber);
        machine.PermitSeries = Normalize(request.PermitSeries);
        machine.PermitExpiresAt = request.PermitExpiresAt!.Value;
        machine.DailySessionLimit = request.DailySessionLimit;
        machine.BetweenSessionCooldownMinutes = request.BetweenSessionCooldownMinutes;
        machine.DailyLimitCooldownMinutes = request.DailyLimitCooldownMinutes;
        machine.IsApproved = request.IsApproved;
        machine.IsActive = request.IsActive;
    }

    private static void Apply(MedCenterMachine machine, UpdateMedCenterMachineRequest request)
    {
        machine.AcquisitionType = (DomainType)request.AcquisitionType;
        machine.InventoryNumber = request.InventoryNumber.Trim();
        machine.Name = request.Name.Trim();
        machine.Model = request.Model.Trim();
        machine.SerialNumber = request.SerialNumber.Trim();
        machine.Manufacturer = request.Manufacturer.Trim();
        machine.ManufacturingCountry = Normalize(request.ManufacturingCountry);
        machine.ManufactureYear = request.ManufactureYear;
        machine.CertificateHolder = Normalize(request.CertificateHolder);
        machine.CertificateHolderCountry = Normalize(request.CertificateHolderCountry);
        machine.CertificateNumber = Normalize(request.CertificateNumber);
        machine.CertificateCountry = Normalize(request.CertificateCountry);
        machine.CertificateIssuedAt = request.CertificateIssuedAt!.Value;
        machine.PermitName = Normalize(request.PermitName);
        machine.PermitNumber = Normalize(request.PermitNumber);
        machine.PermitSeries = Normalize(request.PermitSeries);
        machine.PermitExpiresAt = request.PermitExpiresAt!.Value;
        machine.DailySessionLimit = request.DailySessionLimit;
        machine.BetweenSessionCooldownMinutes = request.BetweenSessionCooldownMinutes;
        machine.DailyLimitCooldownMinutes = request.DailyLimitCooldownMinutes;
        machine.IsApproved = request.IsApproved;
        machine.IsActive = request.IsActive;
    }

    private async Task AddMachineLogAsync(
        long userId,
        string action,
        string? entityId,
        int statusCode,
        bool succeeded,
        string? failureReason,
        string tenantId,
        CancellationToken cancellationToken)
    {
        await _actionLogService.AddAsync(new ActionLogRequest
        {
            UserId = userId,
            Action = action,
            Module = "medcenter",
            EntityName = "MedCenterMachine",
            EntityId = entityId,
            StatusCode = statusCode,
            Succeeded = succeeded,
            FailureReason = failureReason,
            MetadataJson = $$"""{"tenantId":"{{tenantId}}"}"""
        }, cancellationToken);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static MedCenterMachineDto ToDto(MedCenterMachine machine)
    {
        return new MedCenterMachineDto(
            machine.Id,
            machine.TenantId,
            (ContractType)machine.AcquisitionType,
            machine.InventoryNumber,
            machine.Name,
            machine.Model,
            machine.SerialNumber,
            machine.Manufacturer,
            machine.ManufacturingCountry,
            machine.ManufactureYear,
            machine.CertificateHolder,
            machine.CertificateHolderCountry,
            machine.CertificateNumber,
            machine.CertificateCountry,
            machine.CertificateIssuedAt,
            machine.PermitName,
            machine.PermitNumber,
            machine.PermitSeries,
            machine.PermitExpiresAt,
            machine.DailySessionLimit,
            machine.BetweenSessionCooldownMinutes,
            machine.DailyLimitCooldownMinutes,
            machine.IsApproved,
            machine.IsActive);
    }
}