using Application.Audit;
using Application.Auth;
using Application.Common;
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
    private readonly IRequestContext _requestContext;
    private readonly IActionLogService _actionLogService;

    public AuthService(
        AppDbContext db,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService,
        IConfiguration configuration,
        IRequestContext requestContext,
        IActionLogService actionLogService)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _configuration = configuration;
        _requestContext = requestContext;
        _actionLogService = actionLogService;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Username == request.Username, cancellationToken);
        if (user is null || !user.IsActive)
        {
            await _actionLogService.AddAsync(new ActionLogRequest
            {
                UserId = null,
                UsernameSnapshot = request.Username,
                Action = "LoginFailed",
                Module = "auth",
                StatusCode = 401,
                Succeeded = false,
                FailureReason = "Invalid credentials"
            }, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return null;
        }

        var hasher = new PasswordHasher<User>();
        var passwordResult = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (passwordResult == PasswordVerificationResult.Failed)
        {
            user.FailedLoginCount++;
            await _actionLogService.AddAsync(new ActionLogRequest
            {
                UserId = user.Id,
                UsernameSnapshot = user.Username,
                Action = "LoginFailed",
                Module = "auth",
                StatusCode = 401,
                Succeeded = false,
                FailureReason = "Invalid credentials"
            }, cancellationToken);
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
            CreatedByIp = _requestContext.IpAddress,
            UserAgent = _requestContext.UserAgent
        });

        user.FailedLoginCount = 0;
        user.LastLoginAt = DateTimeOffset.UtcNow;
        await _actionLogService.AddAsync(new ActionLogRequest
        {
            UserId = user.Id,
            UsernameSnapshot = user.Username,
            Action = "LoginSucceeded",
            Module = "auth",
            StatusCode = 200,
            Succeeded = true
        }, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return  new LoginResponse(accessToken.Token,plainRefreshToken,accessToken.ExpiresAt,new AuthUserDto(user.Id,user.Username,user.FirstName,user.LastName,permissions));
    }

    public async Task<LoginResponse?> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var tokenHash = _refreshTokenService.HashToken(request.RefreshToken);
        var refreshToken = await _db.RefreshTokens.Include(x=>x.User).FirstOrDefaultAsync(x=>x.TokenHash == tokenHash, cancellationToken);
        if (refreshToken is null || refreshToken.RevokedAt is not null || refreshToken.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            await _actionLogService.AddAsync(new ActionLogRequest
            {
                UserId = null,
                UsernameSnapshot = null,
                Action = "RefreshTokenFailed",
                Module = "auth",
                StatusCode = 401,
                Succeeded = false,
                FailureReason = "Invalid refresh token"
            }, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return null;
        }
        var user = refreshToken.User;
        if (!user.IsActive)
        {
            await _actionLogService.AddAsync(new ActionLogRequest
            {
                UserId = user.Id,
                UsernameSnapshot = user.Username,
                Action = "RefreshTokenFailed",
                Module = "auth",
                StatusCode = 401,
                Succeeded = false,
                FailureReason = "Inactive user"
            }, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return null;
        }
        refreshToken.RevokedAt = DateTimeOffset.UtcNow;
        refreshToken.RevokedByIp = _requestContext.IpAddress;
        var permissions = await LoadPermissionsAsync(user.Id, cancellationToken);
        var plainRefreshToken = _refreshTokenService.CreatePlainToken();
        var refreshDays = int.TryParse(_configuration["JWT_REFRESH_TOKEN_DAYS"], out var days) ? days : 1;
        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _refreshTokenService.HashToken(plainRefreshToken),
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(refreshDays),
            CreatedByIp = _requestContext.IpAddress,
            UserAgent = _requestContext.UserAgent
        });
        var accessToken = _jwtTokenService.CreateAccessToken(user, permissions);
        await _actionLogService.AddAsync(new ActionLogRequest
        {
            UserId = user.Id,
            UsernameSnapshot = user.Username,
            Action = "RefreshTokenSucceeded",
            Module = "auth",
            StatusCode = 200,
            Succeeded = true,
            FailureReason = null
        }, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return new LoginResponse(accessToken.Token,plainRefreshToken,accessToken.ExpiresAt,new AuthUserDto(user.Id,user.Username,user.FirstName,user.LastName,permissions));
    }

    public async Task<AuthUserDto?> GetMeAsync(long userId, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return null;
        }
        var permissions = await LoadPermissionsAsync(user.Id, cancellationToken);
        return new AuthUserDto(user.Id,user.Username,user.FirstName,user.LastName,permissions);
    }

    public async Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken)
    {
        var tokenHash = _refreshTokenService.HashToken(request.RefreshToken);
        var refreshToken = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
        if (refreshToken is not null && refreshToken.RevokedAt is null)
        {
            refreshToken.RevokedAt = DateTimeOffset.UtcNow;
            refreshToken.RevokedByIp = _requestContext.IpAddress;
        }
        await _actionLogService.AddAsync(new ActionLogRequest
        {
            UserId = _requestContext.UserId,
            UsernameSnapshot = _requestContext.Username,
            Action = "LogoutSucceeded",
            Module = "auth",
            StatusCode = 200,
            Succeeded = true
        }, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<List<string>> LoadPermissionsAsync(long userId, CancellationToken cancellationToken)
    {
        return await _db.UserRoles.AsNoTracking().Where(x=>x.UserId == userId).SelectMany(x=>x.Role.RolePermissions).Select(x=>x.Permission.Code).Distinct().ToListAsync(cancellationToken);
    }
}