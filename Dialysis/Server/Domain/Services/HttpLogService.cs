using Dialysis.Shared.Dto;
using Dialysis.Shared.Models;
using Dialysis.Shared.Params;
using Microsoft.EntityFrameworkCore;

namespace Dialysis.Server.Domain.Services
{
    public interface IHttpLogService
    {
        Task<PagedHttpLogResponse> GetPagedAsync(HttpLogParams parameters);
        Task<HttpLogDto?> GetByIdAsync(long id);
    }

    public class HttpLogService : IHttpLogService
    {
        private readonly AppDbContext _db;

        public HttpLogService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<PagedHttpLogResponse> GetPagedAsync(HttpLogParams parameters)
        {
            var query = _db.Set<HttpLog>().AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(parameters.SearchString))
            {
                var search = parameters.SearchString.ToLower();
                query = query.Where(l =>
                    (l.Path != null && l.Path.ToLower().Contains(search)) ||
                    (l.IpAddress != null && l.IpAddress.Contains(search)) ||
                    (l.UserName != null && l.UserName.ToLower().Contains(search)) ||
                    (l.HttpMethod.ToLower().Contains(search)));
            }

            if (!string.IsNullOrEmpty(parameters.HttpMethod))
                query = query.Where(l => l.HttpMethod == parameters.HttpMethod.ToUpper());

            if (parameters.StatusCode.HasValue)
                query = query.Where(l => l.StatusCode == parameters.StatusCode.Value);

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
                "httpmethod" => parameters.SortAsc ? query.OrderBy(l => l.HttpMethod) : query.OrderByDescending(l => l.HttpMethod),
                "path" => parameters.SortAsc ? query.OrderBy(l => l.Path) : query.OrderByDescending(l => l.Path),
                "statuscode" => parameters.SortAsc ? query.OrderBy(l => l.StatusCode) : query.OrderByDescending(l => l.StatusCode),
                "durationms" => parameters.SortAsc ? query.OrderBy(l => l.DurationMs) : query.OrderByDescending(l => l.DurationMs),
                "ipaddress" => parameters.SortAsc ? query.OrderBy(l => l.IpAddress) : query.OrderByDescending(l => l.IpAddress),
                _ => query.OrderByDescending(l => l.Timestamp)
            };

            var items = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PagedHttpLogResponse
            {
                Items = items.Select(MapToDto).ToList(),
                TotalItems = totalItems
            };
        }

        public async Task<HttpLogDto?> GetByIdAsync(long id)
        {
            var log = await _db.Set<HttpLog>().AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
            return log == null ? null : MapToDto(log);
        }

        private static HttpLogDto MapToDto(HttpLog log) => new()
        {
            Id = log.Id,
            Timestamp = log.Timestamp,
            IpAddress = log.IpAddress,
            HttpMethod = log.HttpMethod,
            Path = log.Path,
            QueryString = log.QueryString,
            RequestHeaders = log.RequestHeaders,
            RequestBody = log.RequestBody,
            StatusCode = log.StatusCode,
            ResponseHeaders = log.ResponseHeaders,
            ResponseBody = log.ResponseBody,
            DurationMs = log.DurationMs,
            UserId = log.UserId,
            UserName = log.UserName
        };
    }
}
