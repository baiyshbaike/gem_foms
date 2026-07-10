using Application.Auth;
using Contracts.Auth;
using Domain.Audit;
using Domain.Users;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Auth;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IConfiguration  _configuration;

    public AuthService(AppDbContext db, IJwtTokenService jwtTokenService, IRefreshTokenService refreshTokenService,
        IConfiguration configuration)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _configuration = configuration;
    }
    
    public async Task<LoginResponse?> LoginAsync(LoginRequest request, string? ipAddress, string? userAgent, string correlationId, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Username == request.Username, cancellationToken);
        if (user is null || !user.IsActive)
        {
            WriteActionLog(null, request.Username, "LoginFailed",false,"Invalid credentials",ipAddress,userAgent,correlationId);
            await _db.SaveChangesAsync(cancellationToken);
            return null;
        }

        var hasher = new PasswordHasher<User>();
        var passwordResult = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (passwordResult == PasswordVerificationResult.Failed)
        {
            user.FailedLoginCount++;
            WriteActionLog(user.Id,user.Username,"LoginFailed",false,"Invalid credentials",ipAddress, userAgent, correlationId);
            await _db.SaveChangesAsync(cancellationToken);
        }

        var permissions = await LoadPermissionsAsync(user.Id, cancellationToken);
        var accessToken = _jwtTokenService.CreateAccessToken(user, permissions);
        var plainRefreshToken = _refreshTokenService.CreatePlainToken();
        var refreshTokenHash = _refreshTokenService.HashToken(plainRefreshToken);
        var refreshDays = int.TryParse(_configuration["JWT_REFRESH_TOKEN_DAYS"], out var days) ? days : 1;
        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(refreshDays),
            CreatedByIp = ipAddress,
            UserAgent = userAgent
        });

        user.FailedLoginCount = 0;
        user.LastLoginAt = DateTimeOffset.UtcNow;
        WriteActionLog(user.Id, user.Username, "LoginSucceeded", true, null, ipAddress, userAgent, correlationId);
        await _db.SaveChangesAsync(cancellationToken);
        return  new LoginResponse(accessToken.Token,plainRefreshToken,accessToken.ExpiresAt,new AuthUserDto(user.Id,user.Username,user.FirstName,user.LastName,permissions));
    }

    private async Task<List<string>> LoadPermissionsAsync(long userId, CancellationToken cancellationToken)
    {
        return await _db.UserRoles.AsNoTracking().Where(x=>x.UserId == userId).SelectMany(x=>x.Role.RolePermissions).Select(x=>x.Permission.Code).Distinct().ToListAsync(cancellationToken);
    }

    private void WriteActionLog(long? userId, string? username, string action, bool succeeded, string? failureReason, string? ipAddress, string? userAgent, string correlationId)
    {
        _db.ActionLogs.Add(new ActionLog
        {
            UserId = userId,
            UsernameSnapshot = username,
            Action = action,
            Module = "auth",
            IpAddress = ipAddress,
            UserAgent = userAgent,
            StatusCode = succeeded ? 200 : 401,
            Succeeded = succeeded,
            FailureReason = failureReason,
            CorrelationId = correlationId,
            CreatedAt = DateTimeOffset.UtcNow,
        });
    }
}