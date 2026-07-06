using System.Text.Json;
using Dialysis.Shared.Dto;
using Dialysis.Shared.Models;
using Dialysis.Shared.Params;
using Microsoft.EntityFrameworkCore;

namespace Dialysis.Server.Domain.Services
{
    public interface IActionLogService
    {
        Task<PagedActionLogResponse> GetPagedAsync(ActionLogParams parameters);
        Task<ActionLogDto?> GetByIdAsync(long id);
        Task LogAsync(long? userId, string? userName, string? userIp, string actionType,
            string? entityType, string? entityId, object? oldValues, object? newValues, string? description);
    }

    public class ActionLogService : IActionLogService
    {
        private readonly AppDbContext _db;
        private readonly ILogSettingsProvider _settingsProvider;

        public ActionLogService(AppDbContext db, ILogSettingsProvider settingsProvider)
        {
            _db = db;
            _settingsProvider = settingsProvider;
        }

        public async Task<PagedActionLogResponse> GetPagedAsync(ActionLogParams parameters)
        {
            var query = _db.Set<ActionLog>().AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(parameters.SearchString))
            {
                var search = parameters.SearchString.ToLower();
                query = query.Where(l =>
                    (l.UserName != null && l.UserName.ToLower().Contains(search)) ||
                    (l.ActionType.ToLower().Contains(search)) ||
                    (l.EntityType != null && l.EntityType.ToLower().Contains(search)) ||
                    (l.Description != null && l.Description.ToLower().Contains(search)));
            }

            if (!string.IsNullOrEmpty(parameters.ActionType))
                query = query.Where(l => l.ActionType == parameters.ActionType);

            if (!string.IsNullOrEmpty(parameters.EntityType))
                query = query.Where(l => l.EntityType == parameters.EntityType);

            if (parameters.DateFrom.HasValue)
                query = query.Where(l => l.Timestamp >= parameters.DateFrom.Value);

            if (parameters.DateTo.HasValue)
                query = query.Where(l => l.Timestamp <= parameters.DateTo.Value);

            if (parameters.UserId.HasValue)
                query = query.Where(l => l.UserId == parameters.UserId.Value);

            var totalItems = await query.CountAsync();

            query = (parameters.SortBy?.ToLower()) switch
            {
                "timestamp" => parameters.SortAsc ? query.OrderBy(l => l.Timestamp) : query.OrderByDescending(l => l.Timestamp),
                "actiontype" => parameters.SortAsc ? query.OrderBy(l => l.ActionType) : query.OrderByDescending(l => l.ActionType),
                "entitytype" => parameters.SortAsc ? query.OrderBy(l => l.EntityType) : query.OrderByDescending(l => l.EntityType),
                "username" => parameters.SortAsc ? query.OrderBy(l => l.UserName) : query.OrderByDescending(l => l.UserName),
                _ => query.OrderByDescending(l => l.Timestamp)
            };

            var items = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PagedActionLogResponse
            {
                Items = items.Select(MapToDto).ToList(),
                TotalItems = totalItems
            };
        }

        public async Task<ActionLogDto?> GetByIdAsync(long id)
        {
            var log = await _db.Set<ActionLog>().AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
            return log == null ? null : MapToDto(log);
        }

        public async Task LogAsync(long? userId, string? userName, string? userIp, string actionType,
            string? entityType, string? entityId, object? oldValues, object? newValues, string? description)
        {
            var settings = await _settingsProvider.GetSettingsAsync();
            if (settings != null && !settings.LogActionEnabled)
                return;

            var log = new ActionLog
            {
                Timestamp = DateTime.Now,
                UserId = userId,
                UserName = userName,
                UserIp = userIp,
                ActionType = actionType,
                EntityType = entityType,
                EntityId = entityId,
                OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
                Description = description
            };

            _db.Set<ActionLog>().Add(log);
            await _db.SaveChangesAsync();
        }

        private static ActionLogDto MapToDto(ActionLog log) => new()
        {
            Id = log.Id,
            Timestamp = log.Timestamp,
            UserId = log.UserId,
            UserName = log.UserName,
            UserIp = log.UserIp,
            ActionType = log.ActionType,
            EntityType = log.EntityType,
            EntityId = log.EntityId,
            OldValues = log.OldValues,
            NewValues = log.NewValues,
            Description = log.Description
        };
    }
}
