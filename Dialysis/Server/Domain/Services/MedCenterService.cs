using System;
using System.Security.Claims;
using Dialysis.Server.Domain;
using Dialysis.Shared.Params;
using Dialysis.Shared.Responses;
using Microsoft.AspNetCore.Identity;
using Dialysis.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.PortableExecutable;
using DevExpress.Data.ODataLinq.Helpers;
using Dialysis.Shared.Dto;
using DocumentFormat.OpenXml.Spreadsheet;
using static MudBlazor.CategoryTypes;

namespace Dialysis.Server.Domain.Services
{
    public interface IMedCenterService
    {
        Task<IRetResult> AddMedcenter(MedCenter model);
        Task<Result<List<MedCenter>>> AllMedcenter();
        Task<Result<List<MedCenterChartDto>>> MedCentersChart();

        Task<IRetResult> AddMachine(MachineAddParams model);
        Task<Result<List<MedCenterMachine>>> MedCenterMachines(long  MedcenterId);
        Task<Result<List<MedCenterEmployee>>> AllEmployees(long  MedcenterId);
        Task<IRetResult> DeleteEmployee(MedCenterEmployee model);
        Task<IRetResult> AddEditEmployee(MedCenterEmployee model);
        Task<Result<List<MedCenter>>> UserMedcenter(long userId);
        Task<IRetResult> AddRemoveUser(long Id, long userId = 0, long status = 0);
        Task<IRetResult> AddIndicator(IndicatorPostArgs model);
        Task<Result<List<Indicator>>> AllIndicator(long MedCenterId);
        Task<Result<IndicatorPostArgs>> IndDetail(long indId);
        Task<Result<List<MedCenterMachineDto>>> AllMachinesDto();
        Task<Result<List<MedCentersByDistrictDto>>> MedCentersByDistrict();
        Task<Result<List<PatientAgeGroupDto>>> PatientByAgeGroup();
        Task<IRetResult> DeleteMashineById(long mashineId);
    }

    public class MedCenterService : IMedCenterService
    {
          private readonly AppDbContext _dbContext;
          private readonly IActiveUserService _currentUser;
          private readonly IActionLogService _actionLogService;
          
          public async Task<IRetResult> DeleteMashineById(long id)
        {
            if (id != null)
            {

                var dateNow = DateTime.Now;
                var activeSession = await _dbContext.MedCenterMachine.Where(p => p.Id.Equals(id)).FirstOrDefaultAsync();
                if (activeSession != null)
                {
                      _dbContext.Remove(activeSession);
                      await _dbContext.SaveChangesAsync();
                      await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Delete", "MedCenterMachine", activeSession.Id.ToString(), null, null, null);
                  }
                  else
                  {
                      return await Result<int>.FailAsync("Аппарат не найден");
                  }
            }
            else
            {
                return await Result<int>.FailAsync("Аппарат не найден");
            }

            return await Result<int>.SuccessAsync();
        }
          public MedCenterService(AppDbContext dbContext, IActiveUserService currentUser, IActionLogService actionLogService)
          {
              _dbContext = dbContext;
              _currentUser = currentUser;
              _actionLogService = actionLogService;
          }

        public async Task<IRetResult> AddMedcenter(MedCenter model)
        {
            if (model != null)
            {
                if (model.Id != null && model.CreatedOn != null)
                {
                    var localModel = await _dbContext.MedCenter.Where(t => t.Id.Equals(model.Id)).FirstOrDefaultAsync();
                    if (localModel != null)
                    {
                        localModel.Title = model.Title;
                        localModel.RegionId = model.RegionId;
                        localModel.DistrictId = model.DistrictId;
                        localModel.Address = model.Address;
                        localModel.Phone = model.Phone;
                        localModel.Phone2 = model.Phone2;
                        localModel.IsDeleted = false;
                      _dbContext.Attach(localModel);
                          await _dbContext.SaveChangesAsync();
                          await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "MedCenter", localModel.Id.ToString(), null, null, null);
                      }
                  }
                  else
                  {
                      model.CreatedBy = _currentUser.UserId;
                    model.CreatedOn = DateTime.Now;
                    model.IsDeleted = false;
                      await _dbContext.AddAsync(model);
                      await _dbContext.SaveChangesAsync();
                      await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "MedCenter", model.Id.ToString(), null, null, null);
                  }
              }
              return await Result<int>.SuccessAsync();          
          }

        public async Task<Result<List<MedCenter>>> AllMedcenter()
        {
            var retData = await _dbContext.MedCenter.ToListAsync();           
            return await Result<List<MedCenter>>.SuccessAsync(retData);
        }
        public async Task<Result<List<MedCenterChartDto>>> MedCentersChart()
        {
            var retData = new List<MedCenterChartDto>();
            var dists = await _dbContext.MedCenter.OrderBy(p => p.Title).ToListAsync();
            if (dists != null)
            {
                foreach (var dist in dists)
                {
                    var newObj = new MedCenterChartDto();
                    var total = await _dbContext.MedCard.Where(t => t.MedCenterId.Equals(dist.Id)).CountAsync();
                    newObj.Total = total;
                    newObj.MedCenterTittle = dist.Title;
                    retData.Add(newObj);   

                }
            }
            return await Result<List<MedCenterChartDto>>.SuccessAsync(retData);
        }


        public async Task<IRetResult> AddMachine(MachineAddParams model)
        {
            bool isFileChanged = false;
            var machine = model.MedCenterMachine;
            if(!String.IsNullOrEmpty(model.FileName) && model.FileContent != null)
            {
                var path = "wwwroot/UploadMachine/" + model.FileName;
                var fs = System.IO.File.Create(path);
                fs.Write(model.FileContent, 0, model.FileContent.Length);
                fs.Close();
                machine.UpFile1 = model.FileName;
                isFileChanged = true;
            }
            if (machine != null)
            {
                
                if (machine.Id != null && machine.CreatedOn!=null)
                {
                    var localModel = await _dbContext.MedCenterMachine.Where(t => t.Id.Equals(machine.Id)).FirstOrDefaultAsync();
                    if (localModel != null)
                    {
                        localModel.Model = machine.Model;
                        localModel.Name = machine.Name;
                        localModel.Number = machine.Number;
                        localModel.Manufacturer = machine.Manufacturer;
                        localModel.ManCountry = machine.ManCountry;
                        localModel.ManDate = machine.ManDate;
                        localModel.CertificateHolder = machine.CertificateHolder;
                        localModel.CertificateHolderCountry = machine.CertificateHolderCountry;
                        localModel.CertificateDate = machine.CertificateDate;
                        localModel.PermitName = machine.PermitName;
                        localModel.PermitNumber = machine.PermitNumber;
                        localModel.PermitSeria = machine.PermitSeria;
                        localModel.PermitDate = machine.PermitDate;
                        localModel.LicenseCountry = machine.LicenseCountry;
                        localModel.TotalSessions = machine.TotalSessions;
                        localModel.CertificateNumber = machine.CertificateNumber;
                        localModel.CertificateCountry = machine.CertificateCountry;
                        localModel.CertificateCountry = machine.CertificateCountry;
                        localModel.IsActive = machine.IsActive;
                        localModel.IsApproved = machine.IsApproved;
                        localModel.LastModifiedBy = _currentUser.UserId;
                        localModel.LastModifiedOn = DateTime.Now;
                        if (isFileChanged == true) { localModel.UpFile1 = machine.UpFile1; }
                          _dbContext.Attach(localModel);
                          await _dbContext.SaveChangesAsync();
                          await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "MedCenterMachine", localModel.Id.ToString(), null, null, null);
                      }
                  }
                  else
                  {
                      machine.CreatedBy = _currentUser.UserId;
                    machine.CreatedOn = DateTime.Now;
                    machine.IsDeleted = false;
                      await _dbContext.AddAsync(machine);
                      await _dbContext.SaveChangesAsync();
                      await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "MedCenterMachine", machine.Id.ToString(), null, null, null);
                  }
              }
              return await Result<int>.SuccessAsync();
          }

        public async Task<Result<List<MedCenterMachine>>> MedCenterMachines(long MedCenterId)
        {
            var isadmin = _currentUser.User.IsInRole("admin");
            var retData = await _dbContext.MedCenterMachine
                .Where(t => (t.MedCenterId == MedCenterId || MedCenterId == 0) && (t.IsActive || isadmin)).OrderBy(m => m.Number).ToListAsync();
            return await Result<List<MedCenterMachine>>.SuccessAsync(retData);
        }

        public async Task<Result<List<MedCenterEmployee>>> AllEmployees(long MedCenterId)
        {
            var retData = await _dbContext.MedCenterEmployee
                .Where(t => (t.MedCenterId == MedCenterId || MedCenterId == 0)).ToListAsync();
            return await Result<List<MedCenterEmployee>>.SuccessAsync(retData);
        }
        public async Task<IRetResult> DeleteEmployee(MedCenterEmployee model)
        {
            if (model != null)
            {

                if (model.Id != null && model.Id != 0 && model.CreatedOn != null)
                {
                    var localModel = await _dbContext.MedCenterEmployee.Where(t => t.Id.Equals(model.Id)).FirstOrDefaultAsync();
                    if (localModel != null)
                    {
                          _dbContext.Remove(localModel);
                          await _dbContext.SaveChangesAsync();
                          await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Delete", "MedCenterEmployee", localModel.Id.ToString(), null, null, null);
                      }
                  }
              }
              return await Result<int>.SuccessAsync();     
          }
        public async Task<IRetResult> AddEditEmployee(MedCenterEmployee model)
        {
            if(model != null)
            {
               
                    if (model.Id != null && model.Id != 0 && model.CreatedOn != null)
                    {
                        var localModel = await _dbContext.MedCenterEmployee.Where(t => t.Id.Equals(model.Id)).FirstOrDefaultAsync();
                        if (localModel != null)
                        {
                            localModel.FirstName = model.FirstName;
                            localModel.LastName = model.LastName;
                            localModel.JobTitle = model.JobTitle;
                            localModel.PhoneNumber = model.PhoneNumber;
                            localModel.MedCenterId = model.MedCenterId;
                            localModel.MiddleName = model.MiddleName;
                            localModel.LastModifiedBy = _currentUser.UserId;
                            localModel.FullName = $"{model.FirstName} {model.LastName} {model.MiddleName}";
                            localModel.LastModifiedOn = DateTime.Now;
                            if (!string.IsNullOrEmpty(model.UpFile1) && !string.IsNullOrEmpty(model.FileContent))
                            {
                                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UploadFiles", "Employees");
                                if (!Directory.Exists(uploadPath))
                                {
                                    Directory.CreateDirectory(uploadPath);
                                }
                                
                                var filePath = Path.Combine(uploadPath, model.UpFile1);
                                await System.IO.File.WriteAllBytesAsync(filePath, Convert.FromBase64String(model.FileContent));
                                localModel.UpFile1 = model.UpFile1;
                            }
                              _dbContext.Attach(localModel);
                              await _dbContext.SaveChangesAsync();
                              await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "MedCenterEmployee", localModel.Id.ToString(), null, null, null);
                          }
                      }
                      else
                      {
                          var patientOld = await _dbContext.MedCenterEmployee.Where(p => p.Id.Equals(model.Id)).FirstOrDefaultAsync();
                        if (patientOld != null)
                        {
                            return await Result<int>.FailAsync("Пациент с таким ИНН существует");
                        }
                        else
                        {
                            model.FullName = $"{model.FirstName} {model.LastName} {model.MiddleName}";
                            model.CreatedBy = _currentUser.UserId;
                            model.CreatedOn = DateTime.Now;
                            model.IsDeleted = false;
                            if (!string.IsNullOrEmpty(model.UpFile1) && !string.IsNullOrEmpty(model.FileContent))
                            {
                                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UploadFiles", "Employees");
                                if (!Directory.Exists(uploadPath))
                                {
                                    Directory.CreateDirectory(uploadPath);
                                }
                                
                                var filePath = Path.Combine(uploadPath, model.UpFile1);
                                await System.IO.File.WriteAllBytesAsync(filePath, Convert.FromBase64String(model.FileContent));
                            }
                              await _dbContext.AddAsync(model);
                              await _dbContext.SaveChangesAsync();
                              await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "MedCenterEmployee", model.Id.ToString(), null, null, null);
                          }                   
                      }
            }
            return await Result<int>.SuccessAsync();          
        }
        public async Task<Result<List<MedCenterMachineDto>>> AllMachinesDto()
        {
            var retData = new List<MedCenterMachineDto>();
            var allVal = await (from u in _dbContext.MedCenterMachine
                          join up in _dbContext.MedCenter on u.MedCenterId equals up.Id
                          select new MedCenterMachineDto()
                          {
                              MedCenterMachine = u,
                              MedCenter = up
                          }
                           ).ToListAsync();
            if (allVal != null)
            {
                var grp = allVal.GroupBy(p => new { p.MedCenterMachine.Model, p.MedCenterMachine.Manufacturer}).Select(group => new MedCenterMachineDto()
                {
                    MedCenterMachine = new MedCenterMachine()
                    {
                        Id = group.First().MedCenterMachine.Id,
                        Manufacturer = group.First().MedCenterMachine.Manufacturer,
                        Model = group.First().MedCenterMachine.Model,
                        Name = group.First().MedCenterMachine.Model + " " + group.First().MedCenterMachine.Name,
                        ManCountry = group.First().MedCenterMachine.ManCountry,
                        CertificateHolder = group.First().MedCenterMachine.CertificateHolder,
                        CertificateHolderCountry = group.First().MedCenterMachine.CertificateHolderCountry,
                        CertificateNumber = group.First().MedCenterMachine.CertificateNumber + " " + group.First().MedCenterMachine.CertificateDate,
                        CertificateDate = group.First().MedCenterMachine.CertificateDate,
                        PermitName = group.First().MedCenterMachine.PermitName + " " + group.First().MedCenterMachine.PermitNumber + " " + group.First().MedCenterMachine.PermitSeria + " " + group.First().MedCenterMachine.PermitDate,
                       
                        TotalSessions = group.First().MedCenterMachine.TotalSessions,
                        PermitNumber = group.Where(p => p.MedCenterMachine.IsActive.Equals(true)).Count().ToString(),
                        Number = group.Where(p => p.MedCenterMachine.IsActive.Equals(false)).Count().ToString(),
                    },
                    MedCenter = group.First().MedCenter
                }).ToList();

                retData.AddRange(grp);
            }
            
            return await Result<List<MedCenterMachineDto>>.SuccessAsync(retData);
        }

        public async Task<Result<List<MedCenter>>> UserMedcenter(long userId)
        {
            try
            {
                var allVal = (from u in _dbContext.MedCenter
                              join up in _dbContext.MedCenterUser on u.Id equals up.MedCenterId
                              where up.UserId.Equals(userId)
                              select u
                           ).OrderBy(p => p.Title).ToList();

                return await Result<List<MedCenter>>.SuccessAsync(allVal);
            }
            catch (Exception ex)
            {            
                return await Result<List<MedCenter>>.FailAsync(ex.Message);
            }
           
        }

        public async Task<IRetResult> AddRemoveUser(long Id, long userId = 0, long status = 0)
        {
            if (userId != 0)
            {
                if (status == 0)
                {
                    var userCenter = await _dbContext.MedCenterUser.Where(t => t.MedCenterId == Id && t.UserId == userId).FirstOrDefaultAsync();
                      _dbContext.Remove(userCenter);
                      await _dbContext.SaveChangesAsync();
                      await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Delete", "MedCenterUser", userCenter.Id.ToString(), null, null, null);
                  }
                  else
                  {
                      var userCenter = new MedCenterUser
                    {
                        UserId = userId,
                        MedCenterId = Id
                    };
                      var addnew = await _dbContext.AddAsync(userCenter);
                      await _dbContext.SaveChangesAsync();
                      await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "MedCenterUser", userCenter.Id.ToString(), null, null, null);
                  }
              }
              
              return await Result<int>.SuccessAsync();


        }

        public async Task<IRetResult> AddCertificates(List<MedCenterFiles> files)
        {
            
            return await Result<int>.SuccessAsync();
        }


        public async Task<IRetResult> AddIndicator(IndicatorPostArgs model)
        {
            if (model != null)
            {
                var indicator = new Indicator();
                indicator.MedCenterId = model.MedCenterId;
                indicator.Title =   model.Title;
                indicator.CreatedBy = _currentUser.UserId;
                indicator.CreatedOn = DateTime.Now;
                indicator.IsDeleted = false;
                indicator.startIndicator = model.startIndicator;
                indicator.endIndicator = model.endIndicator;
                using var transaction = _dbContext.Database.BeginTransaction();
                try
                {
                      var addnew = await _dbContext.AddAsync(indicator);
                      await _dbContext.SaveChangesAsync();
                      await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "Indicator", addnew.Entity.Id.ToString(), null, null, null);
                      var indId = addnew.Entity.Id;
                    if (indId != null)
                    {
                        foreach (var item in model.IndicatorRows)
                        {
                            var newRow = new IndicatorRow()
                            {
                                IndicatorId = indId,
                                ReferenceId = item.ReferenceId,
                                Plan1 = item.Plan1,
                                Plan2 = item.Plan2,
                                Fact1 = item.Fact1,
                                Fact2 = item.Fact2,
                                Change1 = item.Change1,
                                Change2 = item.Change2,
                                Threshold = item.Threshold,
                                CreatedBy = _currentUser.UserId,
                                CreatedOn = DateTime.Now
                            };
                            await _dbContext.AddAsync(newRow);
                            await _dbContext.SaveChangesAsync();
                        }
                        transaction.Commit();
                        return await Result<int>.SuccessAsync();
                    }
                    else
                    {
                        transaction.Rollback();
                        return await Result<int>.FailAsync("Ошибка при сохранении");
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return await Result<int>.FailAsync("Ошибка при сохранении");
                }
            }
            else
            {
                return await Result<int>.FailAsync("Ошибка при сохранении");
            }            
        }

        public async Task<Result<List<Indicator>>> AllIndicator(long MedCenterId)
        {
            var retData = await _dbContext.Indicator.Where(t => t.MedCenterId == MedCenterId).ToListAsync();
            return await Result<List<Indicator>>.SuccessAsync(retData);
        }

        public async Task<Result<IndicatorPostArgs>> IndDetail(long indId)
        {
            var retData = new IndicatorPostArgs();
            var indicator = await _dbContext.Indicator.Where(t => t.Id == indId).FirstOrDefaultAsync();
            if(indicator != null )
            {
                retData.Title = indicator.Title;
                retData.MedCenterId = indicator.MedCenterId;
                retData.CreatedOn = indicator.CreatedOn;
                retData.IndicatorRows = await _dbContext.IndicatorRow.Where(t => t.IndicatorId == indicator.Id).ToListAsync();
            }
            return await Result<IndicatorPostArgs>.SuccessAsync(retData);
        }

        public async Task<Result<List<MedCentersByDistrictDto>>> MedCentersByDistrict()
        {
            var retData = new List<MedCentersByDistrictDto>();
            var dists = await _dbContext.District.OrderBy(p => p.Title).ToListAsync();
            if (dists != null)
            {
                foreach (var dist in dists)
                {
                    var newObj = new MedCentersByDistrictDto();
                    newObj.TotalCount = await _dbContext.MedCenter.Where(t => t.DistrictId.Equals(dist.Id)).CountAsync();
                    newObj.RegionName = dist.Title;
                    newObj.RegionId = dist.Id;
                    retData.Add(newObj);   

                }
            }
            return await Result<List<MedCentersByDistrictDto>>.SuccessAsync(retData);
        }

        public async Task<Result<List<PatientAgeGroupDto>>> PatientByAgeGroup()
        {
            var retData = new List<PatientAgeGroupDto>();
            var dists = await _dbContext.Patient.Where(p=>p.GroupId!=2).OrderBy(p => p.Id).ToListAsync();
            
            var dateUnder25 = new DateTime(1999, 1, 1);
            var dateUnder50 = new DateTime(1975, 1, 1);

            var distMale = dists.Where(p => p.Gender.Equals(true)).ToList();
            var distFeMale = dists.Where(p => p.Gender.Equals(false)).ToList();
            var newObj = new PatientAgeGroupDto();
            newObj.Population = distMale.Count(p => p.BirthDate <= dateUnder25);
            newObj.Gender = "Муж.";
            newObj.AgeGroup = "0-25 лет";
            retData.Add(newObj);
            var newObj2 = new PatientAgeGroupDto();
            newObj2.Population = distMale.Count(p => p.BirthDate >= dateUnder25 && p.BirthDate <= dateUnder50);
            newObj2.Gender = "Муж.";
            newObj2.AgeGroup = "25-50 лет";
            retData.Add(newObj2);
            var newObj3 = new PatientAgeGroupDto();
            newObj3.Population = distMale.Count(p => p.BirthDate > dateUnder50);
            newObj3.Gender = "Муж.";
            newObj3.AgeGroup = "50 лет и старше";
            retData.Add(newObj3);


            var newObj4 = new PatientAgeGroupDto();
            newObj4.Population = distFeMale.Count(p => p.BirthDate <= dateUnder25);
            newObj4.Gender = "Жен.";
            newObj4.AgeGroup = "0-25 лет";
            retData.Add(newObj4);

            var newObj5 = new PatientAgeGroupDto();
            newObj5.Population = distFeMale.Count(p => p.BirthDate >= dateUnder25 && p.BirthDate <= dateUnder50);
            newObj5.Gender = "Жен.";
            newObj5.AgeGroup = "25-50 лет";
            retData.Add(newObj5);

            var newObj6 = new PatientAgeGroupDto();
            newObj6.Population = distFeMale.Count(p => p.BirthDate > dateUnder50);
            newObj6.Gender = "Жен.";
            newObj6.AgeGroup = "50 лет и старше";
            retData.Add(newObj6);


            return await Result<List<PatientAgeGroupDto>>.SuccessAsync(retData);
        }

        
    }
}

