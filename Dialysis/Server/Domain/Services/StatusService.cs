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
    public interface IStatusService
    {
        Task<IRetResult> AddStatus(Status model);
        Task<Result<List<Status>>> AllStatus();

        Task<IRetResult> AddGroup(PatientGroup model);
        Task<Result<List<PatientGroup>>> AllGroups();

        Task<IRetResult> AddPrice(ActivePrice model);
        Task<Result<List<ActivePrice>>> AllPrices();

        Task<IRetResult> AddAnalysis(Analysis model);
        Task<Result<List<Analysis>>> AllAnalysis();
        Task<IRetResult> AddCodeMKBs(CodeMKB model);
        Task<IRetResult> AddMedicine(MedicineType model);
        Task<Result<List<MedicineType>>> AllMedicine();
        Task<IRetResult> AddDialyzer(DialyzerType model);
        Task<Result<List<DialyzerType>>> AllDialyzer();
        Task<Result<List<CodeMKB>>> AllCodeMKBs();
        Task<IRetResult> AddReason(GroupReason model);
        Task<Result<List<GroupReason>>> AllReason();
        Task<IRetResult> AddLPU(GroupLPU model);
        Task<Result<List<GroupLPU>>> AllLPU();
        Task<Result<List<IndicatorReference>>> AllIndRef();
        Task<IRetResult> AddIndRef(IndicatorReference model);
        Task<IRetResult> AddPersonTitle(GroupPersonTitle model);
        Task<Result<List<GroupPersonTitle>>> AllPersonTitle();
        Task<IRetResult> AddAkt1(QualityExam1 model);
        Task<IRetResult> AddAkt2(Exam2Dto model);
        Task<IRetResult> AddAkt3(Exam3Dto model);

        Task<Result<List<QualityExam1>>> AllAkt1();
        Task<Result<List<Exam2Dto>>> AllAkt2();
        Task<Result<List<Exam3Dto>>> AllAkt3();
    }

    public class StatusService : IStatusService
    {
          private readonly AppDbContext _dbContext;
          private readonly IActiveUserService _currentUser;
          private readonly IActionLogService _actionLogService;

          public StatusService(AppDbContext dbContext, IActiveUserService currentUser, IActionLogService actionLogService)
          {
              _dbContext = dbContext;
              _currentUser = currentUser;
              _actionLogService = actionLogService;
          }

        public async Task<IRetResult> AddStatus(Status model)
        {
            if(model != null)
            {
                model.CreatedBy = _currentUser.UserId;
                model.CreatedOn = DateTime.Now;
            }
              var addnew = await _dbContext.AddAsync(model);
              await _dbContext.SaveChangesAsync();
              await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "Status", addnew.Entity.Id.ToString(), null, null, null);
              return await Result<int>.SuccessAsync();          
          }

        public async Task<Result<List<Status>>> AllStatus()
        {
            var retData = await _dbContext.Status.ToListAsync();           
            return await Result<List<Status>>.SuccessAsync(retData);
        }


        public async Task<IRetResult> AddGroup(PatientGroup model)
        {
            if (model != null)
            {
                if (model.Id != null && model.CreatedOn != null)
                {
                    var localModel = await _dbContext.PatientGroup.Where(t => t.Id.Equals(model.Id)).FirstOrDefaultAsync();
                    if (localModel != null)
                    {
                        localModel.Title = model.Title;                        
                        _dbContext.Attach(localModel);
                        await _dbContext.SaveChangesAsync();
                        await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "PatientGroup", localModel.Id.ToString(), null, null, null);
                    }
                }
                else
                {
                    model.CreatedBy = _currentUser.UserId;
                    model.CreatedOn = DateTime.Now;
                    model.IsDeleted = false;
                    await _dbContext.AddAsync(model);
                    await _dbContext.SaveChangesAsync();
                    await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "PatientGroup", model.Id.ToString(), null, null, null);
                }
            }
            return await Result<int>.SuccessAsync();
        }

        public async Task<Result<List<PatientGroup>>> AllGroups()
        {
            var retData = await _dbContext.PatientGroup.ToListAsync();
            return await Result<List<PatientGroup>>.SuccessAsync(retData);
        }

        public async Task<IRetResult> AddPrice(ActivePrice model)
        {

            var sqlText = "UPDATE public.\"ActivePrice\" SET \"IsDeleted\"=true, \"DeletedOn\"='"+DateTime.Now +"' WHERE \"IsDeleted\"=false;";
            await _dbContext.Database.ExecuteSqlRawAsync(sqlText);           


            if (model != null)
            {
                model.CreatedBy = _currentUser.UserId;
                model.CreatedOn = DateTime.Now;
                model.IsDeleted = false;
            }
              await _dbContext.AddAsync(model);
              await _dbContext.SaveChangesAsync();
              await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "ActivePrice", model.Id.ToString(), null, null, null);
              return await Result<int>.SuccessAsync();
          }

        public async Task<Result<List<ActivePrice>>> AllPrices()
        {
            var retData = await _dbContext.ActivePrice.OrderByDescending(p=>p.CreatedOn).ToListAsync();
            return await Result<List<ActivePrice>>.SuccessAsync(retData);
        }


        public async Task<IRetResult> AddAnalysis(Analysis model)
        {
            if (model != null)
            {
                if (model.Id != null && model.CreatedOn != null)
                {
                    var localModel = await _dbContext.Analysise.Where(t => t.Id.Equals(model.Id)).FirstOrDefaultAsync();
                    if (localModel != null)
                    {
                        localModel.Title = model.Title;
                        localModel.AnalysisExt = model.AnalysisExt;
                        localModel.Note2 = model.Note2;
                        localModel.NextDays = model.NextDays;
                        localModel.Description = model.Description;                     
                        localModel.LastModifiedBy = _currentUser.UserId;
                        localModel.LastModifiedOn = DateTime.Now;
                        _dbContext.Attach(localModel);
                        await _dbContext.SaveChangesAsync();
                        await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "Analysis", localModel.Id.ToString(), null, null, null);
                    }
                }
                else
                {
                    model.CreatedBy = _currentUser.UserId;
                    model.CreatedOn = DateTime.Now;
                    model.IsDeleted = false;
                    await _dbContext.AddAsync(model);
                    await _dbContext.SaveChangesAsync();
                    await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "Analysis", model.Id.ToString(), null, null, null);
                }
            }
            return await Result<int>.SuccessAsync();
           
        }
        public async Task<IRetResult> AddCodeMKBs(CodeMKB model)
        {
            if (model != null)
            {
                if (model.Id != null && model.CreatedOn != null)
                {
                    var localModel = await _dbContext.CodeMKBs.Where(t => t.Id.Equals(model.Id)).FirstOrDefaultAsync();
                    if (localModel != null)
                    {
                        localModel.Code = model.Code;
                        localModel.Name = model.Name;
                        localModel.AgeProperty = model.AgeProperty;
                        localModel.Pol = model.Pol;
                        localModel.LastModifiedBy = _currentUser.UserId;
                        localModel.LastModifiedOn = DateTime.Now;
                        _dbContext.Attach(localModel);
                        await _dbContext.SaveChangesAsync();
                        await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "CodeMKB", localModel.Id.ToString(), null, null, null);
                    }
                }
                else
                {
                    model.CreatedBy = _currentUser.UserId;
                    model.CreatedOn = DateTime.Now;
                    model.IsDeleted = false;
                    await _dbContext.AddAsync(model);
                    await _dbContext.SaveChangesAsync();
                    await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "CodeMKB", model.Id.ToString(), null, null, null);
                }
            }
            return await Result<int>.SuccessAsync();

        }
        public async Task<IRetResult> AddMedicine(MedicineType model)
        {
            if (model != null)
            {
                if (model.Id != null && model.CreatedOn != null)
                {
                    var localModel = await _dbContext.MedicineType.Where(t => t.Id.Equals(model.Id)).FirstOrDefaultAsync();
                    if (localModel != null)
                    {
                        localModel.Name = model.Name;
                        localModel.LastModifiedBy = _currentUser.UserId;
                        localModel.LastModifiedOn = DateTime.Now;
                        _dbContext.Attach(localModel);
                        await _dbContext.SaveChangesAsync();
                        await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "MedicineType", localModel.Id.ToString(), null, null, null);
                    }
                }
                else
                {
                    var localModel = await _dbContext.MedicineType.Where(t => t.Name.Equals(model.Name)).FirstOrDefaultAsync();
                    if (localModel == null)
                    {
                        model.CreatedBy = _currentUser.UserId;
                        model.CreatedOn = DateTime.Now;
                        model.IsDeleted = false;
                        await _dbContext.AddAsync(model);
                        await _dbContext.SaveChangesAsync();
                        await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "MedicineType", model.Id.ToString(), null, null, null);
                    }
                }
            }
            return await Result<int>.SuccessAsync();
        }
        public async Task<Result<List<MedicineType>>> AllMedicine()
        {
            var retData = await _dbContext.MedicineType.Where(p => p.IsDeleted.Equals(false)).ToListAsync();
            return await Result<List<MedicineType>>.SuccessAsync(retData);
        }
        public async Task<IRetResult> AddDialyzer(DialyzerType model)
        {
            if (model != null)
            {
                if (model.Id != null && model.CreatedOn != null)
                {
                    var localModel = await _dbContext.DialyzerType.Where(t => t.Id.Equals(model.Id)).FirstOrDefaultAsync();
                    if (localModel != null)
                    {
                        localModel.Name = model.Name;
                        localModel.LastModifiedBy = _currentUser.UserId;
                        localModel.LastModifiedOn = DateTime.Now;
                        _dbContext.Attach(localModel);
                        await _dbContext.SaveChangesAsync();
                        await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "DialyzerType", localModel.Id.ToString(), null, null, null);
                    }
                }
                else
                {
                    var localModel = await _dbContext.DialyzerType.Where(t => t.Name.Equals(model.Name)).FirstOrDefaultAsync();
                    if (localModel == null)
                    {
                        model.CreatedBy = _currentUser.UserId;
                        model.CreatedOn = DateTime.Now;
                        model.IsDeleted = false;
                        await _dbContext.AddAsync(model);
                        await _dbContext.SaveChangesAsync();
                        await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "DialyzerType", model.Id.ToString(), null, null, null);
                    }
                }
            }
            return await Result<int>.SuccessAsync();

        }
        public async Task<Result<List<Analysis>>> AllAnalysis()
        {
            var retData = await _dbContext.Analysise.Where(p=>p.IsDeleted.Equals(false)).ToListAsync();
            return await Result<List<Analysis>>.SuccessAsync(retData);
        }
        public async Task<Result<List<CodeMKB>>> AllCodeMKBs()
        {
            var retData = await _dbContext.CodeMKBs.Where(p => p.IsDeleted.Equals(false)).ToListAsync();
            return await Result<List<CodeMKB>>.SuccessAsync(retData);
        }
        public async Task<Result<List<DialyzerType>>> AllDialyzer()
        {
            var retData = await _dbContext.DialyzerType.Where(p => p.IsDeleted.Equals(false)).ToListAsync();
            return await Result<List<DialyzerType>>.SuccessAsync(retData);
        }
  
        public async Task<IRetResult> AddReason(GroupReason model)
        {

            if (model != null)
            {
                if (model.Id != null && model.CreatedOn != null)
                {
                    var localModel = await _dbContext.GroupReason.Where(t => t.Id.Equals(model.Id)).FirstOrDefaultAsync();
                    if (localModel != null)
                    {
                        localModel.Title = model.Title;
                        _dbContext.Attach(localModel);
                        await _dbContext.SaveChangesAsync();
                        await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "GroupReason", localModel.Id.ToString(), null, null, null);
                    }
                }
                else
                {
                    model.CreatedBy = _currentUser.UserId;
                    model.CreatedOn = DateTime.Now;
                    model.IsDeleted = false;
                    await _dbContext.AddAsync(model);
                    await _dbContext.SaveChangesAsync();
                    await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "GroupReason", model.Id.ToString(), null, null, null);
                }
            }
            return await Result<int>.SuccessAsync();
        }

        public async Task<Result<List<GroupReason>>> AllReason()
        {
            var retData = await _dbContext.GroupReason.OrderBy(p => p.Title).ToListAsync();
            return await Result<List<GroupReason>>.SuccessAsync(retData);
        }

        public async Task<IRetResult> AddLPU(GroupLPU model)
        {

            if (model != null)
            {
                if (model.Id != null && model.CreatedOn != null)
                {
                    var localModel = await _dbContext.GroupLPU.Where(t => t.Id.Equals(model.Id)).FirstOrDefaultAsync();
                    if (localModel != null)
                    {
                        localModel.Title = model.Title;
                        _dbContext.Attach(localModel);
                        await _dbContext.SaveChangesAsync();
                        await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "GroupLPU", localModel.Id.ToString(), null, null, null);
                    }
                }
                else
                {
                    model.CreatedBy = _currentUser.UserId;
                    model.CreatedOn = DateTime.Now;
                    model.IsDeleted = false;
                    await _dbContext.AddAsync(model);
                    await _dbContext.SaveChangesAsync();
                    await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "GroupLPU", model.Id.ToString(), null, null, null);
                }
            }
            return await Result<int>.SuccessAsync();
        }

        public async Task<Result<List<GroupLPU>>> AllLPU()
        {
            var retData = await _dbContext.GroupLPU.OrderBy(p => p.Title).ToListAsync();
            return await Result<List<GroupLPU>>.SuccessAsync(retData);
        }

        public async Task<Result<List<IndicatorReference>>> AllIndRef()
        {
            var retData = await _dbContext.IndicatorReference.OrderBy(p => p.Title).ToListAsync();
            return await Result<List<IndicatorReference>>.SuccessAsync(retData);
        }

        public async Task<IRetResult> AddIndRef(IndicatorReference model)
        {
            if (model != null)
            {
                if (model.Id != null && model.CreatedOn != null)
                {
                    var localModel = await _dbContext.IndicatorReference.Where(t => t.Id.Equals(model.Id)).FirstOrDefaultAsync();
                    if (localModel != null)
                    {
                        localModel.Title = model.Title;
                        _dbContext.Attach(localModel);
                        await _dbContext.SaveChangesAsync();
                        await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "IndicatorReference", localModel.Id.ToString(), null, null, null);
                    }
                }
                else
                {
                    model.CreatedBy = _currentUser.UserId;
                    model.CreatedOn = DateTime.Now;
                    model.IsDeleted = false;
                    await _dbContext.AddAsync(model);
                    await _dbContext.SaveChangesAsync();
                    await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "IndicatorReference", model.Id.ToString(), null, null, null);
                }
            }
            return await Result<int>.SuccessAsync();
        }

        public async Task<IRetResult> AddPersonTitle(GroupPersonTitle model)
        {
            if (model != null)
            {
                if (model.Id != null && model.CreatedOn != null)
                {
                    var localModel = await _dbContext.GroupPersonTitle.Where(t => t.Id.Equals(model.Id)).FirstOrDefaultAsync();
                    if (localModel != null)
                    {
                        localModel.Title = model.Title;
                        _dbContext.Attach(localModel);
                        await _dbContext.SaveChangesAsync();
                        await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "GroupPersonTitle", localModel.Id.ToString(), null, null, null);
                    }
                }
                else
                {
                    model.CreatedBy = _currentUser.UserId;
                    model.CreatedOn = DateTime.Now;
                    model.IsDeleted = false;
                    await _dbContext.AddAsync(model);
                    await _dbContext.SaveChangesAsync();
                    await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "GroupPersonTitle", model.Id.ToString(), null, null, null);
                }
            }
            return await Result<int>.SuccessAsync();
        }

        public async Task<Result<List<GroupPersonTitle>>> AllPersonTitle()
        {
            var retData = await _dbContext.GroupPersonTitle.OrderBy(p => p.Title).ToListAsync();
            return await Result<List<GroupPersonTitle>>.SuccessAsync(retData);
        }

        public async Task<IRetResult> AddAkt1(QualityExam1 model)
        {
            if (model != null)
            {
                model.CreatedBy = _currentUser.UserId;
                model.CreatedOn = DateTime.Now;
                model.IsDeleted = false;
                await _dbContext.AddAsync(model);
                await _dbContext.SaveChangesAsync();
                await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "QualityExam1", model.Id.ToString(), null, null, null);
            }
            return await Result<int>.SuccessAsync();
        }

        public async Task<Result<List<QualityExam1>>> AllAkt1()
        {
            var retData = await _dbContext.QualityExam1.OrderByDescending(p => p.AktDate).ToListAsync();
            return await Result<List<QualityExam1>>.SuccessAsync(retData);
        }

        public async Task<IRetResult> AddAkt2(Exam2Dto model)
        {
            if (model != null)
            {
                //model.CreatedBy = _currentUser.UserId;
                //model.CreatedOn = DateTime.Now;
                //model.IsDeleted = false;
                //await _dbContext.AddAsync(model);
                //await _dbContext.SaveChangesAsync();
            }
            return await Result<int>.SuccessAsync();
        }

        public async Task<IRetResult> AddAkt3(Exam3Dto model)
        {
            if (model != null)
            {
                //model.CreatedBy = _currentUser.UserId;
                //model.CreatedOn = DateTime.Now;
                //model.IsDeleted = false;
                //await _dbContext.AddAsync(model);
                //await _dbContext.SaveChangesAsync();
            }
            return await Result<int>.SuccessAsync();
        }

        public async Task<Result<List<Exam2Dto>>> AllAkt2()
        {
            return null;
            //var retData = await _dbContext.GroupLPU.OrderBy(p => p.Title).ToListAsync();
            //return await Result<List<Exam2Dto>>.SuccessAsync(retData);
        }

        public async Task<Result<List<Exam3Dto>>> AllAkt3()
        {
            return null;
            //var retData = await _dbContext.GroupLPU.OrderBy(p => p.Title).ToListAsync();
            //return await Result<List<Exam2Dto>>.SuccessAsync(retData);
        }

    }
}

