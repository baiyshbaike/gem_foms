using Application.Audit;
using Application.MedCards;
using Application.Tenants;
using Contracts.MedCards;
using Domain.MedCards;
using Domain.Patients;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ContractStatus = Contracts.MedCards.MedCardStatusDto;
using DomainStatus = Domain.MedCards.MedCardStatus;
namespace Infrastructure.MedCards;

public sealed class MedCardService : IMedCardService
{
    private readonly AppDbContext _db;
    private readonly IActionLogService _actionLogService;
    private readonly ITenantAccessService _tenantAccessService;

    public MedCardService(
        AppDbContext db,
        IActionLogService actionLogService,
        ITenantAccessService tenantAccessService)
    {
        _db = db;
        _actionLogService = actionLogService;
        _tenantAccessService = tenantAccessService;
    }

    public async Task<IReadOnlyList<MedCardDto>?> GetAsync(
        long userId,
        IReadOnlyList<string>? tenantIds,
        CancellationToken cancellationToken)
    {
        var filter = await _tenantAccessService.ResolveTenantFilterAsync(userId, tenantIds, cancellationToken);
        if (!filter.Succeeded)
        {
            await _actionLogService.AddAsync(new ActionLogRequest
            {
                UserId = userId,
                Action = "MedCardsViewFailed",
                Module = "medcard",
                EntityName = "MedCard",
                StatusCode = 403,
                Succeeded = false,
                FailureReason = "Tenant filter contains forbidden tenant"
            }, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return null;
        }

        var medCards = await _db.MedCards
            .AsNoTracking()
            .Where(x => filter.TenantIds.Contains(x.TenantId) && !x.IsDeleted)
            .OrderByDescending(x => x.OpenedAt)
            .Select(x => new MedCardDto(
                x.Id,
                x.TenantId,
                x.PatientId,
                x.CardNumber,
                x.OpenedAt,
                x.ClosedAt,
                (ContractStatus)x.Status,
                x.Notes))
            .ToListAsync(cancellationToken);

        await _actionLogService.AddAsync(new ActionLogRequest
        {
            UserId = userId,
            Action = "MedCardsViewed",
            Module = "medcard",
            EntityName = "MedCard",
            StatusCode = 200,
            Succeeded = true,
            MetadataJson = $$"""{"tenantCount":{{filter.TenantIds.Count}},"resultCount":{{medCards.Count}}}"""
        }, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return medCards;
    }

    public async Task<MedCardDto?> GetByIdAsync(
        long userId,
        string tenantId,
        long id,
        CancellationToken cancellationToken)
    {
        if (!await _tenantAccessService.CanAccessTenantAsync(userId, tenantId, cancellationToken))
        {
            await _actionLogService.AddAsync(new ActionLogRequest
            {
                UserId = userId,
                Action = "MedCardViewFailed",
                Module = "medcard",
                EntityName = "MedCard",
                EntityId = id.ToString(),
                StatusCode = 403,
                Succeeded = false,
                FailureReason = "Tenant access denied",
                MetadataJson = $$"""{"tenantId":"{{tenantId}}"}"""
            }, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return null;
        }

        var medCard = await _db.MedCards
            .AsNoTracking()
            .Where(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted)
            .Select(x => new MedCardDto(
                x.Id,
                x.TenantId,
                x.PatientId,
                x.CardNumber,
                x.OpenedAt,
                x.ClosedAt,
                (ContractStatus)x.Status,
                x.Notes))
            .FirstOrDefaultAsync(cancellationToken);

        await _actionLogService.AddAsync(new ActionLogRequest
        {
            UserId = userId,
            Action = medCard is null ? "MedCardViewFailed" : "MedCardViewed",
            Module = "medcard",
            EntityName = "MedCard",
            EntityId = id.ToString(),
            StatusCode = medCard is null ? 404 : 200,
            Succeeded = medCard is not null,
            FailureReason = medCard is null ? "MedCard not found" : null,
            MetadataJson = $$"""{"tenantId":"{{tenantId}}"}"""
        }, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return medCard;
    }

    public async Task<MedCardCommandResult<MedCardDto>> CreateAsync(
        string tenantId,
        long userId,
        CreateMedCardRequest request,
        CancellationToken cancellationToken)
    {
        if (!await _tenantAccessService.CanAccessTenantAsync(userId, tenantId, cancellationToken))
        {
            await _actionLogService.AddAsync(new ActionLogRequest
            {
                UserId = userId,
                Action = "MedCardCreateFailed",
                Module = "medcard",
                EntityName = "MedCard",
                StatusCode = 403,
                Succeeded = false,
                FailureReason = "Tenant access denied",
                MetadataJson = $$"""{"tenantId":"{{tenantId}}","patientId":{{request.PatientId}}}"""
            }, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return new MedCardCommandResult<MedCardDto>(MedCardCommandStatus.Forbidden);
        }

        var patientExists = await _db.Patients.AnyAsync(x =>
            x.Id == request.PatientId &&
            !x.IsDeleted &&
            x.IsActive &&
            x.GroupId != PatientGroupIds.Archive,
            cancellationToken);

        if (!patientExists)
        {
            await _actionLogService.AddAsync(new ActionLogRequest
            {
                UserId = userId,
                Action = "MedCardCreateFailed",
                Module = "medcard",
                EntityName = "MedCard",
                StatusCode = 404,
                Succeeded = false,
                FailureReason = "Patient not found, inactive, or archived",
                MetadataJson = $$"""{"tenantId":"{{tenantId}}","patientId":{{request.PatientId}}}"""
            }, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return new MedCardCommandResult<MedCardDto>(MedCardCommandStatus.NotFound);
        }

        var duplicateForPatient = await _db.MedCards.AnyAsync(x =>
            x.TenantId == tenantId &&
            x.PatientId == request.PatientId &&
            !x.IsDeleted,
            cancellationToken);

        if (duplicateForPatient)
        {
            await _actionLogService.AddAsync(new ActionLogRequest
            {
                UserId = userId,
                Action = "MedCardCreateFailed",
                Module = "medcard",
                EntityName = "MedCard",
                StatusCode = 409,
                Succeeded = false,
                FailureReason = "Patient already has medcard in active tenant",
                MetadataJson = $$"""{"tenantId":"{{tenantId}}","patientId":{{request.PatientId}}}"""
            }, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return new MedCardCommandResult<MedCardDto>(MedCardCommandStatus.Conflict);
        }

        var duplicateCardNumber = await _db.MedCards.AnyAsync(x =>
            x.TenantId == tenantId &&
            x.CardNumber == request.CardNumber &&
            !x.IsDeleted,
            cancellationToken);

        if (duplicateCardNumber)
        {
            await _actionLogService.AddAsync(new ActionLogRequest
            {
                UserId = userId,
                Action = "MedCardCreateFailed",
                Module = "medcard",
                EntityName = "MedCard",
                StatusCode = 409,
                Succeeded = false,
                FailureReason = "Card number already exists in active tenant",
                MetadataJson = $$"""{"tenantId":"{{tenantId}}"}"""
            }, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return new MedCardCommandResult<MedCardDto>(MedCardCommandStatus.Conflict);
        }

        var medCard = new MedCard
        {
            TenantId = tenantId,
            PatientId = request.PatientId,
            CardNumber = request.CardNumber.Trim(),
            OpenedAt = request.OpenedAt ?? DateTimeOffset.UtcNow,
            Status = DomainStatus.Open,
            Notes = request.Notes
        };

        _db.MedCards.Add(medCard);
        await _db.SaveChangesAsync(cancellationToken);

        await _actionLogService.AddAsync(new ActionLogRequest
        {
            UserId = userId,
            Action = "MedCardCreated",
            Module = "medcard",
            EntityName = "MedCard",
            EntityId = medCard.Id.ToString(),
            StatusCode = 201,
            Succeeded = true,
            MetadataJson = $$"""{"tenantId":"{{tenantId}}","patientId":{{medCard.PatientId}}}"""
        }, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return new MedCardCommandResult<MedCardDto>(MedCardCommandStatus.Succeeded, ToDto(medCard));
    }

    public async Task<MedCardCommandResult<MedCardDto>> UpdateAsync(
        string tenantId,
        long id,
        long userId,
        UpdateMedCardRequest request,
        CancellationToken cancellationToken)
    {
        if (!await _tenantAccessService.CanAccessTenantAsync(userId, tenantId, cancellationToken))
        {
            await _actionLogService.AddAsync(new ActionLogRequest
            {
                UserId = userId,
                Action = "MedCardUpdateFailed",
                Module = "medcard",
                EntityName = "MedCard",
                EntityId = id.ToString(),
                StatusCode = 403,
                Succeeded = false,
                FailureReason = "Tenant access denied",
                MetadataJson = $$"""{"tenantId":"{{tenantId}}"}"""
            }, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return new MedCardCommandResult<MedCardDto>(MedCardCommandStatus.Forbidden);
        }

        var medCard = await _db.MedCards
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, cancellationToken);

        if (medCard is null)
        {
            await _actionLogService.AddAsync(new ActionLogRequest
            {
                UserId = userId,
                Action = "MedCardUpdateFailed",
                Module = "medcard",
                EntityName = "MedCard",
                EntityId = id.ToString(),
                StatusCode = 404,
                Succeeded = false,
                FailureReason = "MedCard not found",
                MetadataJson = $$"""{"tenantId":"{{tenantId}}"}"""
            }, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return new MedCardCommandResult<MedCardDto>(MedCardCommandStatus.NotFound);
        }

        var status = (DomainStatus)request.Status;
        if ((status is DomainStatus.Closed or DomainStatus.Archived) && request.ClosedAt is null)
        {
            await _actionLogService.AddAsync(new ActionLogRequest
            {
                UserId = userId,
                Action = "MedCardUpdateFailed",
                Module = "medcard",
                EntityName = "MedCard",
                EntityId = id.ToString(),
                StatusCode = 409,
                Succeeded = false,
                FailureReason = "ClosedAt is required for closed or archived medcard",
                MetadataJson = $$"""{"tenantId":"{{tenantId}}","status":"{{status}}"}"""
            }, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return new MedCardCommandResult<MedCardDto>(MedCardCommandStatus.Conflict);
        }

        var duplicateCardNumber = await _db.MedCards.AnyAsync(x =>
            x.Id != id &&
            x.TenantId == tenantId &&
            x.CardNumber == request.CardNumber &&
            !x.IsDeleted,
            cancellationToken);

        if (duplicateCardNumber)
        {
            await _actionLogService.AddAsync(new ActionLogRequest
            {
                UserId = userId,
                Action = "MedCardUpdateFailed",
                Module = "medcard",
                EntityName = "MedCard",
                EntityId = id.ToString(),
                StatusCode = 409,
                Succeeded = false,
                FailureReason = "Card number already exists in active tenant",
                MetadataJson = $$"""{"tenantId":"{{tenantId}}"}"""
            }, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return new MedCardCommandResult<MedCardDto>(MedCardCommandStatus.Conflict);
        }

        medCard.CardNumber = request.CardNumber.Trim();
        medCard.OpenedAt = request.OpenedAt;
        medCard.ClosedAt = status is DomainStatus.Closed or DomainStatus.Archived ? request.ClosedAt : null;
        medCard.Status = status;
        medCard.Notes = request.Notes;

        await _actionLogService.AddAsync(new ActionLogRequest
        {
            UserId = userId,
            Action = "MedCardUpdated",
            Module = "medcard",
            EntityName = "MedCard",
            EntityId = medCard.Id.ToString(),
            StatusCode = 200,
            Succeeded = true,
            MetadataJson = $$"""{"tenantId":"{{tenantId}}"}"""
        }, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return new MedCardCommandResult<MedCardDto>(MedCardCommandStatus.Succeeded, ToDto(medCard));
    }

    public async Task<MedCardCommandResult<bool>> DeleteAsync(
        string tenantId,
        long id,
        long userId,
        CancellationToken cancellationToken)
    {
        if (!await _tenantAccessService.CanAccessTenantAsync(userId, tenantId, cancellationToken))
        {
            await _actionLogService.AddAsync(new ActionLogRequest
            {
                UserId = userId,
                Action = "MedCardDeleteFailed",
                Module = "medcard",
                EntityName = "MedCard",
                EntityId = id.ToString(),
                StatusCode = 403,
                Succeeded = false,
                FailureReason = "Tenant access denied",
                MetadataJson = $$"""{"tenantId":"{{tenantId}}"}"""
            }, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return new MedCardCommandResult<bool>(MedCardCommandStatus.Forbidden, false);
        }

        var medCard = await _db.MedCards
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, cancellationToken);

        if (medCard is null)
        {
            await _actionLogService.AddAsync(new ActionLogRequest
            {
                UserId = userId,
                Action = "MedCardDeleteFailed",
                Module = "medcard",
                EntityName = "MedCard",
                EntityId = id.ToString(),
                StatusCode = 404,
                Succeeded = false,
                FailureReason = "MedCard not found",
                MetadataJson = $$"""{"tenantId":"{{tenantId}}"}"""
            }, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return new MedCardCommandResult<bool>(MedCardCommandStatus.NotFound, false);
        }

        medCard.IsDeleted = true;

        await _actionLogService.AddAsync(new ActionLogRequest
        {
            UserId = userId,
            Action = "MedCardDeleted",
            Module = "medcard",
            EntityName = "MedCard",
            EntityId = medCard.Id.ToString(),
            StatusCode = 204,
            Succeeded = true,
            MetadataJson = $$"""{"tenantId":"{{tenantId}}"}"""
        }, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return new MedCardCommandResult<bool>(MedCardCommandStatus.Succeeded, true);
    }

    private static MedCardDto ToDto(MedCard medCard)
    {
        return new MedCardDto(
            medCard.Id,
            medCard.TenantId,
            medCard.PatientId,
            medCard.CardNumber,
            medCard.OpenedAt,
            medCard.ClosedAt,
            (ContractStatus)medCard.Status,
            medCard.Notes);
    }
}
