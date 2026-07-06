using System;
using System.Security.Claims;
using Dialysis.Server.Domain;
using Dialysis.Shared.Params;
using Dialysis.Shared.Responses;
using Microsoft.AspNetCore.Identity;
using Dialysis.Shared.Models;
using Microsoft.EntityFrameworkCore;


namespace Dialysis.Server.Domain.Services
{
    public interface IRegionService
    {
        Task<IRetResult> AddDistrict(District model);
        Task<Result<List<District>>> AllDistricts();

        Task<IRetResult> AddRegion(Region model);
        Task<Result<List<Region>>> DistRegions(long Id);
        Task<Result<List<Region>>> AllRegions();
    }

    public class RegionService : IRegionService
    {
          private readonly AppDbContext _dbContext;
          private readonly IActiveUserService _currentUser;
          private readonly IActionLogService _actionLogService;

          public RegionService(AppDbContext dbContext, IActiveUserService currentUser, IActionLogService actionLogService)
          {
              _dbContext = dbContext;
              _currentUser = currentUser;
              _actionLogService = actionLogService;
          }

        public async Task<IRetResult> AddDistrict(District model)
        {
            if (model != null)
            {
                if (model.Id != null && model.CreatedOn != null)
                {
                    var localModel = await _dbContext.District.Where(t => t.Id.Equals(model.Id)).FirstOrDefaultAsync();
                    if (localModel != null)
                    {
                        localModel.Title = model.Title;
                        localModel.IsDeleted = false;
                        _dbContext.Attach(localModel);
                        await _dbContext.SaveChangesAsync();
                        await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "District", localModel.Id.ToString(), null, null, null);
                    }
                }
                else
                {
                    model.CreatedBy = _currentUser.UserId;
                    model.CreatedOn = DateTime.Now;
                    model.IsDeleted = false;
                    await _dbContext.AddAsync(model);
                    await _dbContext.SaveChangesAsync();
                    await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "District", model.Id.ToString(), null, null, null);
                }
            }

            return await Result<int>.SuccessAsync();          
        }

        public async Task<Result<List<District>>> AllDistricts()
        {
            var retData = await _dbContext.District.ToListAsync();           
            return await Result<List<District>>.SuccessAsync(retData);
        }


        public async Task<IRetResult> AddRegion(Region model)
        {
            if (model != null)
            {
                if (model.Id != null && model.CreatedOn != null)
                {
                    var localModel = await _dbContext.Region.Where(t => t.Id.Equals(model.Id)).FirstOrDefaultAsync();
                    if (localModel != null)
                    {
                        localModel.Title = model.Title;
                        localModel.ParentId = model.ParentId;
                        localModel.IsDeleted = false;
                        _dbContext.Attach(localModel);
                        await _dbContext.SaveChangesAsync();
                        await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "Region", localModel.Id.ToString(), null, null, null);
                    }
                }
                else
                {
                    model.CreatedBy = _currentUser.UserId;
                    model.CreatedOn = DateTime.Now;
                    model.IsDeleted = false;
                    await _dbContext.AddAsync(model);
                    await _dbContext.SaveChangesAsync();
                    await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "Region", model.Id.ToString(), null, null, null);
                }
            }
            return await Result<int>.SuccessAsync();
        }

        public async Task<Result<List<Region>>> DistRegions(long Id)
        {
            var retData = await _dbContext.Region.Where(p=>p.ParentId.Equals(Id)).ToListAsync();
            return await Result<List<Region>>.SuccessAsync(retData);
        }

        public async Task<Result<List<Region>>> AllRegions()
        {
            var retData = await _dbContext.Region.ToListAsync();
            return await Result<List<Region>>.SuccessAsync(retData);
        }


    }
}

