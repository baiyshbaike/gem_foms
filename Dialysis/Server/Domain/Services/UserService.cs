using System;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Dialysis.Server.Domain;
using Dialysis.Shared.Params;
using Dialysis.Shared.Responses;
using Microsoft.AspNetCore.Identity;
using Dialysis.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Dialysis.Shared.Dto;
using System.Security.Cryptography;
using System.Text;

namespace Dialysis.Server.Domain.Services
{
    public interface IUserService
    {
        Task<IRetResult> AddUser(User model);
        Task<IRetResult> GetUserProfile(long id);
        Task<Result<List<UserDto>>> AllUsers();
        Task<Result<List<Role>>> UserRoles(long userId);
        Task<Result<List<Role>>> ProfileRoles(long profileId);
        Task<Result<List<Role>>> AllRoles();
        Task<IRetResult> AddRemoveRole(long roleId, long userId = 0, long profileId = 0, long status = 0);
    }

    public class UserService : IUserService
    {
          private readonly AppDbContext _dbContext;
          private readonly IActiveUserService _currentUser;
          private readonly IActionLogService _actionLogService;

          public UserService(AppDbContext dbContext, IActiveUserService currentUser, IActionLogService actionLogService)
          {
              _dbContext = dbContext;
              _currentUser = currentUser;
              _actionLogService = actionLogService;
          }
        public async Task<IRetResult> GetUserProfile(long id)
        {
            var userprofileid = await _dbContext.User.Where(c=>c.Id == id).FirstOrDefaultAsync();
            var profilename = await _dbContext.UserProfile.Where(c => c.Id == userprofileid.ProfileId).FirstOrDefaultAsync();
            var name = profilename?.Title?.ToString();
            return await Result<string>.SuccessAsync(name);
        }
        private static bool IsStrongPassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8) return false;
            if (!password.Any(char.IsDigit)) return false;
            if (!password.Any(char.IsUpper)) return false;
            if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]")) return false;
            return true;
        }

        public async Task<IRetResult> AddUser(User model)
        {
            if (model != null)
            {

                if (model.Id > 0)
                {
                    var user = _dbContext.User.Where(p => p.Username.Equals(model.Username) && p.Id != model.Id).FirstOrDefault();
                    if (user != null)
                    {
                        return await Result<LoginResponse>.FailAsync("Пользователь с таким логином существует!");
                    }

                    var localModel = await _dbContext.User.Where(t => t.Id.Equals(model.Id)).FirstOrDefaultAsync();
                    if (localModel != null)
                    {
                        localModel.LastModifiedBy = _currentUser.UserId;
                        localModel.LastModifiedOn = DateTime.Now;
                        localModel.Username = model.Username;
                        localModel.FirstName = model.FirstName;
                        localModel.LastName = model.LastName;
                        localModel.MIddleName = model.MIddleName;
                        localModel.Email = model.Email;
                        localModel.ProfileId = model.ProfileId;
                        localModel.IsActive = model.IsActive;
                        if (!string.IsNullOrEmpty(model.Password))
                        {
                            if (!IsStrongPassword(model.Password))
                                return await Result<LoginResponse>.FailAsync("Пароль должен содержать минимум 8 символов, заглавную букву, цифру и специальный символ.");
                            localModel.Password = BCrypt.Net.BCrypt.HashPassword(model.Password, workFactor: 12);
                        }                        
                          _dbContext.Attach(localModel);
                          await _dbContext.SaveChangesAsync();
                          await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "User", localModel.Id.ToString(), null, null, null);
                      }
                      
                      return await Result<int>.SuccessAsync();
                }
                else
                {

                    var user = _dbContext.User.Where(p => p.Username.Equals(model.Username)).FirstOrDefault();
                    if (user != null)
                    {
                        return await Result<LoginResponse>.FailAsync("Пользователь с таким логином существует!");
                    }
                    model.CreatedBy = (long)_currentUser.UserId;
                    model.CreatedOn = DateTime.Now;
                    model.IsDeleted = false;
                    model.IsActive = true;
                    if (!IsStrongPassword(model.Password))
                        return await Result<LoginResponse>.FailAsync("Пароль должен содержать минимум 8 символов, заглавную букву, цифру и специальный символ.");
                    model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password, workFactor: 12);

                    var userRoles = (from ur in _dbContext.Role
                                     join r in _dbContext.UserProfileRole on ur.Id equals r.RoleId
                                     where r.ProfileId.Equals(model.ProfileId)
                                     select ur).ToList();


                    using var transaction = _dbContext.Database.BeginTransaction();
                    try
                    {
                        var addnew = await _dbContext.AddAsync(model);
                        await _dbContext.SaveChangesAsync();

                        foreach (var item in userRoles)
                        {
                            var userRole = new UserRole
                            {
                                UserId = addnew.Entity.Id,
                                RoleId = item.Id
                            };
                            await _dbContext.AddAsync(userRole);
                            await _dbContext.SaveChangesAsync();
                        }
                          transaction.Commit();
                          await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "User", addnew.Entity.Id.ToString(), null, null, null);
                          return await Result<int>.SuccessAsync();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return await Result<int>.FailAsync("Transaction Error");
                    }
                }
            }
            else
            {
                return await Result<int>.FailAsync("Model not found");
            }          
           
        }

        public async Task<Result<List<UserDto>>> AllUsers()
        {
            var allUsers = (from u in _dbContext.User
                             join up in _dbContext.UserProfile on u.ProfileId equals up.Id
                             into uprofiles
                             from uprofile in uprofiles.DefaultIfEmpty()
                             select new UserDto
                             {
                                 User = u,
                                 UserProfile = uprofile,
                                 UserRoles = null
                             }
                            ).OrderBy(p=>p.User.FirstName).ToList();

            foreach (var item in allUsers)
            {
                item.MedCenters = (from u in _dbContext.MedCenterUser
                                         join up in _dbContext.MedCenter on u.MedCenterId equals up.Id
                                         where u.UserId.Equals(item.User.Id)
                                         select up
                            ).OrderBy(p => p.Title).ToList();
                item.SaveFiles = (from u in _dbContext.SaveFile where u.EntityId.Equals(item.User.Id) select u)
                    .OrderBy(p => p.Id).ToList();
            }


            return await Result<List<UserDto>>.SuccessAsync(allUsers);
        }

        public async Task<Result<List<Role>>> UserRoles(long userId)
        {
            var allRoles = (from u in _dbContext.Role
                            join up in _dbContext.UserRole on u.Id equals up.RoleId
                            where up.UserId.Equals(userId)
                            select u
                            ).OrderBy(p => p.Name).ToList();

            return await Result<List<Role>>.SuccessAsync(allRoles);
        }

        public async Task<Result<List<Role>>> ProfileRoles(long profileId)
        {
            var allRoles = (from u in _dbContext.Role
                            join up in _dbContext.UserProfileRole on u.Id equals up.RoleId
                            where up.ProfileId.Equals(profileId)
                            select u
                            ).OrderBy(p => p.Name).ToList();

            return await Result<List<Role>>.SuccessAsync(allRoles);
        }

        public async Task<Result<List<Role>>> AllRoles()
        {
            var roles = await _dbContext.Role.Where(p=>p.IsDeleted == false).ToListAsync();
            return await Result<List<Role>>.SuccessAsync(roles);
        }

        public async Task<IRetResult> AddRemoveRole(long roleId, long userId=0, long profileId=0, long status=0)
        {
            if (userId != 0)
            {
                if (status == 0)
                {
                    var userRole = await _dbContext.UserRole.Where(t => t.RoleId == roleId && t.UserId == userId).FirstOrDefaultAsync();
                       _dbContext.Remove(userRole);
                      await _dbContext.SaveChangesAsync();
                      await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Delete", "UserRole", userRole.Id.ToString(), null, null, null);
                  }
                  else
                  {
                      var userRole = new UserRole
                    {
                        UserId = userId,
                        RoleId = roleId
                    };
                      var addnew = await _dbContext.AddAsync(userRole);
                      await _dbContext.SaveChangesAsync();
                      await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "UserRole", userRole.Id.ToString(), null, null, null);
                  }
              }

              if (profileId != 0)
            {
                if (status == 0)
                {
                    var profileRole = await _dbContext.UserProfileRole.Where(t => t.RoleId == roleId && t.ProfileId == profileId).FirstOrDefaultAsync();
                      _dbContext.Remove(profileRole);
                      await _dbContext.SaveChangesAsync();
                      await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Delete", "UserProfileRole", profileRole.Id.ToString(), null, null, null);
                  }
                  else
                  {
                      var profileRole = new UserProfileRole
                    {
                        ProfileId = profileId,
                        RoleId = roleId
                    };
                      var addnew = await _dbContext.AddAsync(profileRole);
                      await _dbContext.SaveChangesAsync();
                      await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "UserProfileRole", profileRole.Id.ToString(), null, null, null);
                  }
              }
              return await Result<int>.SuccessAsync();
            

        }

        // MD5 kept only for reference — new passwords use BCrypt
        private string GetMd5Hash(string input)
        {
            using MD5 md5Hash = MD5.Create();
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
                sBuilder.Append(data[i].ToString("x2"));
            return sBuilder.ToString();
        }

    }
}

