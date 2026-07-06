using System;
using System.Security.Claims;
using Dialysis.Server.Domain;
using Dialysis.Shared.Params;
using Dialysis.Shared.Responses;
using Microsoft.AspNetCore.Identity;
using Dialysis.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Dialysis.Shared.Dto;

namespace Dialysis.Server.Domain.Services
{
    public interface IUserProfileService
    {
        Task<IRetResult> AddProfile(UserProfile model);
        Task<Result<UserDto>> GetUserByProfileId();
        Task<Result<List<UserProfile>>> AllProfiles();
    }

    public class UserProfileService : IUserProfileService
    {
          private readonly AppDbContext _dbContext;
          private readonly IActiveUserService _currentUser;
          private readonly IActionLogService _actionLogService;

          public UserProfileService(AppDbContext dbContext, IActiveUserService currentUser, IActionLogService actionLogService)
          {
              _dbContext = dbContext;
              _currentUser = currentUser;
              _actionLogService = actionLogService;
          }

        public async Task<IRetResult> AddProfile(UserProfile model)
        {
            if(model != null)
            {
                //model.CreatedBy = (long)_currentUser.UserId;
                model.CreatedOn = DateTime.Now;
            }
              var addnew = await _dbContext.AddAsync(model);
              await _dbContext.SaveChangesAsync();
              await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "UserProfile", addnew.Entity.Id.ToString(), null, null, null);
              return await Result<int>.SuccessAsync();          
        }

        public async Task<Result<List<UserProfile>>> AllProfiles()
        {
            var userProfiles = await _dbContext.UserProfile.ToListAsync();           
            return await Result<List<UserProfile>>.SuccessAsync(userProfiles);
        }

        public async Task<Result<UserDto>> GetUserByProfileId()
        {
            UserDto user = new UserDto();
            var value = (from u in _dbContext.User
                            join up in _dbContext.UserProfile on u.ProfileId equals up.Id
                            where(u.Id == _currentUser.UserId)
                            select new UserDto
                            {
                                User = u,
                                UserProfile = up,
                                UserRoles = null
                            }
                            ).FirstOrDefault();
            if (value != null)
            {
                user.User = value.User;
                user.UserRoles = value.UserRoles;
                user.UserProfile = value.UserProfile;
                user.MedCenters = (from u in _dbContext.MedCenterUser
                                   join up in _dbContext.MedCenter on u.MedCenterId equals up.Id
                                   where u.UserId.Equals(_currentUser.UserId)
                                   select new MedCenter
                                   {
                                       Title = up.Title,
                                       Id = up.Id,
                                       DistrictId = up.DistrictId,
                                   }).ToList();
            }
            return await Result<UserDto>.SuccessAsync(user);
        }
    }
}

