using System;
using Dialysis.Shared.Responses;
using IdentityModel.Client;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Dialysis.Shared.Params;
using Dialysis.Shared.Models.Config;
using Dialysis.Server.Domain;
using Dialysis.Shared.Models;
using Microsoft.Extensions.Configuration;

namespace Dialysis.Server.Domain.Services
{

    public interface ILoginService
    {
        Task<Result<LoginResponse>> LoginAsync(LoginParams model, string ipAddress);
        Task<string> Md5Async(string tt);

    }


    public class LoginService : ILoginService
    {
        private const string GenericErrorMessage = "Неверный логин или пароль";
        private const int MaxFailedAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

          private readonly AppDbContext _dbContext;
          private readonly IConfiguration _configuration;
          private readonly ITokenBlacklistService _tokenBlacklistService;
          private readonly IActionLogService _actionLogService;

          public LoginService(
              AppDbContext dbContext,
              IConfiguration configuration,
              ITokenBlacklistService tokenBlacklistService,
              IActionLogService actionLogService)
          {
              _dbContext = dbContext;
              _configuration = configuration;
              _tokenBlacklistService = tokenBlacklistService;
              _actionLogService = actionLogService;
          }

        public async Task<string> Md5Async(string tt)
        {           
            return GetMd5Hash(tt);
        }

        public async Task<Result<LoginResponse>> LoginAsync(LoginParams model, string ipAddress)
        {
            var user = _dbContext.User.Where(p => p.Username.Equals(model.Email)).FirstOrDefault();

            if (user == null)
            {
                return await Result<LoginResponse>.FailAsync(GenericErrorMessage);
            }

            if (!user.IsActive)
            {
                return await Result<LoginResponse>.FailAsync("Пользователь не активен. Пожалуйста, свяжитесь с администратором.");
            }

            // Check lockout
            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.Now)
            {
                var remaining = (int)(user.LockoutEnd.Value - DateTime.Now).TotalMinutes + 1;

                return await Result<LoginResponse>.FailAsync($"Аккаунт временно заблокирован. Повторите попытку через {remaining} мин.");
            }

            // Verify password — supports both legacy MD5 and new BCrypt hashes
            bool isPasswordValid = false;
            bool needsBCryptMigration = false;

            if (IsBCryptHash(user.Password))
            {
                // Already migrated to BCrypt
                isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.Password);
            }
            else
            {
                // Legacy MD5 hash — verify with MD5
                string hashedPass = GetMd5Hash(model.Password);
                isPasswordValid = user.Password.Equals(hashedPass);
                if (isPasswordValid)
                {
                    needsBCryptMigration = true;
                }
            }

            if (!isPasswordValid)
            {
                // Increment failed attempt counter
                user.AccessFailedCount++;
                if (user.AccessFailedCount >= MaxFailedAttempts)
                {
                    user.LockoutEnd = DateTime.Now.Add(LockoutDuration);

                    user.AccessFailedCount = 0;
                }
                await _dbContext.SaveChangesAsync();
                return await Result<LoginResponse>.FailAsync(GenericErrorMessage);
            }

            // On-the-fly migration: re-hash MD5 password with BCrypt
            if (needsBCryptMigration)
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(model.Password, workFactor: 12);
            }

            // Handle active session — "Last login wins" policy.
            // Old sessions are automatically invalidated by incrementing TokenVersion 
            // and deactivating them in the database during SetActiveSessionAsync.
            var activeSession = await _tokenBlacklistService.GetActiveSessionAsync(user.Id);
            if (activeSession != null)
            {
                // If session is expired, we can record the expiry time as last logout.
                if (activeSession.TokenExpiry <= DateTime.Now)
                {
                    user.LastLogoutDate = activeSession.TokenExpiry;
                }
            }

            // Increment token version to invalidate any pre-restart tokens
            user.TokenVersion++;

            // Successful login — reset lockout counters
            user.AccessFailedCount = 0;
            user.LockoutEnd = null;
            user.LastIp = ipAddress;
            user.LastLogin = DateTime.Now;
              await _dbContext.SaveChangesAsync();

              await _actionLogService.LogAsync(user.Id, user.FirstName ?? user.Username, ipAddress, "Login", "User", user.Id.ToString(), null, null, null);

              var (token, jti, expires) = await GenerateJwtAsync(user, ipAddress);
            await _tokenBlacklistService.SetActiveSessionAsync(user.Id, jti, expires, user.TokenVersion, ipAddress);
            var response = new LoginResponse { Token = token, Fio = user.FirstName, ProfileName = user.LastName! };
            return await Result<LoginResponse>.SuccessAsync(response);
        }

        private async Task<(string token, string jti, DateTime expires)> GenerateJwtAsync(User user, string ipAddress)
        {
            var claims = await GetClaimsAsync(user);
            var claimsList = claims.ToList();
            var jti = Guid.NewGuid().ToString();
            var expires = DateTime.Now.AddHours(10);


            claimsList.Add(new Claim("ip", ipAddress));
            claimsList.Add(new Claim("jti", jti));

            var token = GenerateEncryptedToken(GetSigningCredentials(), claimsList, expires);
            return (token, jti, expires);
        }

        private async Task<IEnumerable<Claim>> GetClaimsAsync(User user)
        {
            try
            {
                var userRoles = (from ur in _dbContext.Role
                            join r in _dbContext.UserRole on ur.Id equals r.RoleId
                            where r.UserId.Equals(user.Id)
                            select ur).ToList();
                

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new(ClaimTypes.GivenName, user.Username),
                    new(ClaimTypes.Name, user.FirstName),
                    new(ClaimTypes.Surname, user.LastName),
                    new("tokenVersion", user.TokenVersion.ToString())
                };

                foreach (var role in userRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.Name!.ToString()));
                }

                return claims;
                // return null;
            }
            catch (Exception ex)
            {
                return null;
            }
            
        }

        private string GenerateEncryptedToken(SigningCredentials signingCredentials, IEnumerable<Claim> claims, DateTime expires)
        {
            var token = new JwtSecurityToken(
               claims: claims,
               expires: expires,
               signingCredentials: signingCredentials);
            var tokenHandler = new JwtSecurityTokenHandler();
            var encryptedToken = tokenHandler.WriteToken(token);
            return encryptedToken;
        }


        private SigningCredentials GetSigningCredentials()
        {
            var secretKey = _configuration["AppConfiguration:Secret"] ?? "DEFAULT_SECRET_PLEASE_CHANGE_IN_PRODUCTION";
            var secret = Encoding.UTF8.GetBytes(secretKey);
            return new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256);
        }
        private string GetMd5Hash(string input)
        {
            using MD5 md5Hash = MD5.Create();
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        /// <summary>
        /// Checks if a hash is BCrypt format ($2a$, $2b$, $2y$ prefix, 60 chars).
        /// Legacy MD5 hashes are 32 hex characters.
        /// </summary>
        private static bool IsBCryptHash(string hash)
        {
            return !string.IsNullOrEmpty(hash) &&
                   hash.Length == 60 &&
                   (hash.StartsWith("$2a$") || hash.StartsWith("$2b$") || hash.StartsWith("$2y$"));
        }
    }
}

