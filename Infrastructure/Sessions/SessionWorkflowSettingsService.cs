using Application.Audit;
using Application.Sessions;
using Contracts.Sessions;
using Domain.Sessions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Sessions;

public sealed class SessionWorkflowSettingsService : ISessionWorkflowSettingsService
{
    private readonly AppDbContext _db;
    private readonly IActionLogService _actionLogService;

    public SessionWorkflowSettingsService(AppDbContext db, IActionLogService actionLogService)
    {
        _db = db;
        _actionLogService = actionLogService;
    }

    public async Task<SessionWorkflowSettingsDto> GetAsync(string tenantId, CancellationToken cancellationToken)
    {
        var settings = await _db.SessionWorkflowSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null
            ? ToDto(new SessionWorkflowSettings
            {
                TenantId = tenantId,
                IdentificationStartLimitMinutes = 240,
                AutoFinishActiveMinutes = 270,
                EndIdentificationLimitMinutes = 120,
                SendToPayLimitMinutes = 360
            })
            : ToDto(settings);
    }

    public async Task<SessionWorkflowSettingsDto> UpdateAsync(
        string tenantId,
        long userId,
        UpdateSessionWorkflowSettingsRequest request,
        CancellationToken cancellationToken)
    {
        var settings = await _db.SessionWorkflowSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        if (settings is null)
        {
            settings = new SessionWorkflowSettings
            {
                TenantId = tenantId,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = userId
            };

            _db.SessionWorkflowSettings.Add(settings);
        }

        settings.IdentificationStartLimitMinutes = request.IdentificationStartLimitMinutes;
        settings.AutoFinishActiveMinutes = request.AutoFinishActiveMinutes;
        settings.EndIdentificationLimitMinutes = request.EndIdentificationLimitMinutes;
        settings.SendToPayLimitMinutes = request.SendToPayLimitMinutes;
        settings.UpdatedAt = DateTimeOffset.UtcNow;
        settings.UpdatedBy = userId;

        await _actionLogService.AddAsync(new ActionLogRequest
        {
            UserId = userId,
            Action = "SessionWorkflowSettingsUpdated",
            Module = "settings",
            EntityName = "SessionWorkflowSettings",
            EntityId = tenantId,
            StatusCode = 200,
            Succeeded = true,
            MetadataJson = $$"""{"tenantId":"{{tenantId}}"}"""
        }, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(settings);
    }

    private static SessionWorkflowSettingsDto ToDto(SessionWorkflowSettings settings)
    {
        return new SessionWorkflowSettingsDto(
            settings.Id,
            settings.TenantId,
            settings.IdentificationStartLimitMinutes,
            settings.AutoFinishActiveMinutes,
            settings.EndIdentificationLimitMinutes,
            settings.SendToPayLimitMinutes,
            settings.CreatedAt,
            settings.CreatedBy,
            settings.UpdatedAt,
            settings.UpdatedBy);
    }
}
