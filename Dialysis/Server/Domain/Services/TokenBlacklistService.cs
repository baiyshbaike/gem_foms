using Dialysis.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Dialysis.Server.Domain.Services
{
    public class ActiveSession
    {
        public string Jti { get; set; } = string.Empty;
        public DateTime TokenExpiry { get; set; }
        public int TokenVersion { get; set; }
        public string? IpAddress { get; set; }
    }

    public interface ITokenBlacklistService
    {
        Task BlacklistTokenAsync(string jti);
        Task<bool> IsTokenBlacklistedAsync(string jti);
        Task SetActiveSessionAsync(long userId, string jti, DateTime tokenExpiry, int tokenVersion, string ipAddress);
        Task<ActiveSession?> GetActiveSessionAsync(long userId);
        Task RemoveActiveSessionAsync(long userId);
    }

    public class TokenBlacklistService : ITokenBlacklistService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<TokenBlacklistService> _logger;

        public TokenBlacklistService(AppDbContext dbContext, ILogger<TokenBlacklistService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task BlacklistTokenAsync(string jti)
        {
            var session = await _dbContext.UserSessions
                .FirstOrDefaultAsync(s => s.Jti == jti && s.IsActive);
            if (session != null)
            {
                session.IsActive = false;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<bool> IsTokenBlacklistedAsync(string jti)
        {
            var session = await _dbContext.UserSessions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Jti == jti);
            return session != null && !session.IsActive;
        }

        public async Task SetActiveSessionAsync(long userId, string jti, DateTime tokenExpiry, int tokenVersion, string ipAddress)
        {
            // Deactivate any existing active sessions for this user
            var existingSessions = await _dbContext.UserSessions
                .Where(s => s.UserId == userId && s.IsActive)
                .ToListAsync();

            foreach (var session in existingSessions)
            {
                session.IsActive = false;
            }

            // Create new active session
            _dbContext.UserSessions.Add(new UserSession
            {
                UserId = userId,
                Jti = jti,
                TokenVersion = tokenVersion,
                TokenExpiry = tokenExpiry,
                IpAddress = ipAddress,
                CreatedAt = DateTime.Now,
                IsActive = true
            });

            await _dbContext.SaveChangesAsync();
        }

        public async Task<ActiveSession?> GetActiveSessionAsync(long userId)
        {
            var session = await _dbContext.UserSessions
                .AsNoTracking()
                .Where(s => s.UserId == userId && s.IsActive)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();

            if (session == null)
                return null;

            return new ActiveSession
            {
                Jti = session.Jti,
                TokenExpiry = session.TokenExpiry,
                TokenVersion = session.TokenVersion,
                IpAddress = session.IpAddress
            };
        }

        public async Task RemoveActiveSessionAsync(long userId)
        {
            var sessions = await _dbContext.UserSessions
                .Where(s => s.UserId == userId && s.IsActive)
                .ToListAsync();

            foreach (var session in sessions)
            {
                session.IsActive = false;
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
