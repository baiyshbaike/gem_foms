using System.Security.Claims;
using Dialysis.Shared.Constants;
using Dialysis.Shared.Dto;
using Dialysis.Shared.Models;
using Dialysis.Shared.Responses;
using Microsoft.EntityFrameworkCore;

namespace Dialysis.Server.Domain.Services;

public interface IHDSessionService
{
    Task<SessionResponseDto<HDSessionDto>> GetSessionsPagedAsync(
        DateTime? sessionStartDateFrom,
        DateTime? sessionStartDateTo,
        int pageNumber, 
        int pageSize,
        int statusId, 
        long medCenterId,
        string sortBy, 
        bool sortAsc, 
        string searchString);
    Task<Result<Patient>> SearchByInn(string inn);
    Task<IRetResult> AddIdentify(HDSession hdSession);
    Task<IRetResult> DeleteModel(HDSession hdSession);
    Task<IRetResult> StartHdSession(HDSession hdSession);
    Task<IRetResult> AddEditHourHdSession(HDSessionHour hdSession);
}

public class HDSessionService : IHDSessionService
{
    private readonly AppDbContext _dbContext;
    private readonly IActiveUserService _currentUser;
    private readonly IActionLogService _actionLogService;

    public HDSessionService(AppDbContext dbContext, IActiveUserService currentUser, IActionLogService actionLogService)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _actionLogService = actionLogService;
    }

    public async Task<IRetResult> DeleteModel(HDSession hdSession)
    {
        var localModel = await _dbContext.HDSession.Where(t => t.Id.Equals(hdSession.Id)).FirstOrDefaultAsync();
        if (localModel != null)
        {
            var hdSessionHours = await _dbContext.HDSessionHour
                .Where(t => t.HDSessionId == localModel.Id)
                .ToListAsync();

            if (hdSessionHours.Any())
            {
                _dbContext.HDSessionHour.RemoveRange(hdSessionHours );
            }
            _dbContext.Remove(localModel);
            await _dbContext.SaveChangesAsync();
            await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Delete", "HDSession", localModel.Id.ToString(), new { localModel.Inn, localModel.PatientId }, null, "Сеанс удален");
        }
        return await Result<int>.SuccessAsync();
    }
    public async Task<IRetResult> AddEditHourHdSession(HDSessionHour hdSessionHour)
    {
        if (hdSessionHour.Id == 0)
        {
            hdSessionHour.CreatedBy = _currentUser.UserId;
            hdSessionHour.CreatedOn = DateTime.Now;
            await _dbContext.AddAsync(hdSessionHour);
            await _dbContext.SaveChangesAsync();
            await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "HDSessionHour", hdSessionHour.Id.ToString(), null, new { hdSessionHour.HDSessionId, hdSessionHour.Hour }, "Показатель сеанса добавлен");
            return await Result<int>.SuccessAsync();
        }
        else
        {
            var activeSession = await _dbContext.HDSessionHour.Where(p => p.Id.Equals(hdSessionHour.Id)).FirstOrDefaultAsync();
            if (activeSession != null)
            {
                activeSession.Sys = hdSessionHour.Sys;
                activeSession.Dia = hdSessionHour.Dia;
                activeSession.Ritm = hdSessionHour.Ritm;
                activeSession.Temp = hdSessionHour.Temp;
                activeSession.LastModifiedBy = _currentUser.UserId;
                activeSession.LastModifiedOn = DateTime.Now;
                _dbContext.Attach(activeSession);
                await _dbContext.SaveChangesAsync();
                await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "HDSessionHour", hdSessionHour.Id.ToString(), null, new { hdSessionHour.Sys, hdSessionHour.Dia, hdSessionHour.Ritm, hdSessionHour.Temp }, "Показатель сеанса обновлен");
                return await Result<int>.SuccessAsync();
            }
        }
        return await Result<int>.FailAsync("Not found");
    }
    public async Task<IRetResult> StartHdSession(HDSession hdSession)
    {
        var activeSession = await _dbContext.HDSession.Where(p => p.Id.Equals(hdSession.Id)).FirstOrDefaultAsync();
        if (activeSession != null)
        {
            activeSession.SessionStart = hdSession.SessionStart;
            activeSession.LastModifiedBy = _currentUser.UserId;
            activeSession.LastModifiedOn = DateTime.Now;
            activeSession.Condition = hdSession.Condition;
            activeSession.Complaints = hdSession.Complaints;
            activeSession.Program = hdSession.Program;
            activeSession.Dialyzer = hdSession.Dialyzer;
            activeSession.Correction = hdSession.Correction;
            activeSession.Access = hdSession.Access;
            activeSession.Anticoagulation = hdSession.Anticoagulation;
            activeSession.Uf = hdSession.Uf;
            activeSession.Speed = hdSession.Speed;
            activeSession.Durators = hdSession.Durators;
            activeSession.TypeDialyzer = hdSession.TypeDialyzer;
            activeSession.StartWeight = hdSession.StartWeight;
            activeSession.Sys = hdSession.Sys;
            activeSession.Dia = hdSession.Dia;
            activeSession.Ritm = hdSession.Ritm;
            activeSession.Temp = hdSession.Temp;
            activeSession.Reinfusion = hdSession.Reinfusion;
            _dbContext.Attach(activeSession);
            await _dbContext.SaveChangesAsync();
            await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "HDSession", hdSession.Id.ToString(), null, new { StatusId = "Started", hdSession.SessionStart }, "Сеанс начат");
            return await Result<int>.SuccessAsync();
        }
        return await Result<int>.FailAsync("Not found");
    }
    public async Task<IRetResult> AddIdentify(HDSession hdSession)
    {
        try
        {
            
            var todaySession = await _dbContext.HDSession.Where(p => p.Inn.Equals(hdSession.Inn) && (p.StatusId != (long)HDSessionEnum.Finished || p.StatusId != (long)HDSessionEnum.Payed || p.StatusId != (long)HDSessionEnum.SendToPay) && p.SessionEnd >= DateTime.Today).FirstOrDefaultAsync();
            if (todaySession != null)
            {
                return await Result<int>.FailAsync("Этот пациент прошел протокол сеанса сегодня.");
            }
            var centerSession = await _dbContext.HDSession.Where(p => p.Inn.Equals(hdSession.Inn) && p.StatusId == (long)HDSessionEnum.Identification).FirstOrDefaultAsync();
            if (centerSession != null && (int)(DateTime.Now - (DateTime)centerSession.IdentifyOn).TotalMinutes < 240)
            {
                return await Result<int>.FailAsync("У этого пациента есть идентификация");
            }
            var activeSession = await _dbContext.HDSession.Where(p => p.Inn.Equals(hdSession.Inn) && (p.StatusId.Equals((long)HDSessionEnum.Started) || p.StatusId.Equals((long)HDSessionEnum.Paused))).FirstOrDefaultAsync();
            if (activeSession != null)
            {
                return await Result<int>.FailAsync("У этого пациента есть активный протокол сеанса");
            }
            hdSession.IdentifyBy = _currentUser.UserId;
            hdSession.IdentifyOn = DateTime.Now;
            hdSession.StatusId = (long)HDSessionEnum.Identification;
            if (!String.IsNullOrEmpty(hdSession.ImageStart))
            {
                string img_name = hdSession.PatientId.ToString() + "_" + DateTime.Now.ToString("ddMMyyyyHHmm") + "_start.png";
                string imgPath = "wwwroot/UploadImages/" + img_name;
                string base64 = hdSession.ImageStart;
                base64 = base64.Replace("data:image/jpeg; base64,", "");
                base64 = base64.Replace("data:image/jpeg;base64,", "");
                File.WriteAllBytes(imgPath, Convert.FromBase64String(base64));
                hdSession.ImageStart = img_name;
                await _dbContext.AddAsync(hdSession);
            }
            await _dbContext.AddAsync(hdSession);
            await _dbContext.SaveChangesAsync();
            await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "HDSession", hdSession.Id.ToString(), null, new { hdSession.Inn, hdSession.PatientId, hdSession.MedCenterId, StatusId = "Identification" }, "Сеанс идентифицирован");
            return await Result<int>.SuccessAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    public async Task<Result<Patient>> SearchByInn(string inn)
    {
        var retData = await _dbContext.Patient.Where(p => p.Inn.Equals(inn) && p.IsDeleted.Equals(false)).FirstOrDefaultAsync();
        
        if (retData != null)
        {
            if (retData.GroupId == 2)
            {
                return await Result<Patient>.FailAsync("Пациент в архиве");
            }
            if (retData.GroupId == 8)
            {
                return await Result<Patient>.FailAsync("Это новый пациент");
            }
            var groupText = await _dbContext.PatientGroup.Where(g => g.Id.Equals(retData.GroupId))
                .FirstOrDefaultAsync();
            retData.GroupText = groupText?.Title;
            return await Result<Patient>.SuccessAsync(retData,"Пациент найден");
        }
        return await Result<Patient>.FailAsync("Пациент не найден или не активен");
            
    }
    public async Task<SessionResponseDto<HDSessionDto>> GetSessionsPagedAsync(
        DateTime? sessionStartDateFrom,
        DateTime? sessionStartDateTo,
        int pageNumber, 
        int pageSize,
        int statusId,
        long medCenterId,
        string sortBy, 
        bool sortAsc, 
        string searchString)
    {
        try
        {
            var baseQuery = from hd in _dbContext.HDSession
                            join patient in _dbContext.Patient on hd.PatientId equals patient.Id
                            join medCenter in _dbContext.MedCenter on hd.MedCenterId equals medCenter.Id into medCenterGroup
                            from mc in medCenterGroup.DefaultIfEmpty()
                            join status in _dbContext.Status on hd.StatusId equals status.Id into statusGroup
                            from st in statusGroup.DefaultIfEmpty()
                            join machine in _dbContext.MedCenterMachine on hd.MachineId equals machine.Id into machineGroup
                            from mch in machineGroup.DefaultIfEmpty()
                            join medCard in _dbContext.MedCard on hd.Inn equals medCard.Inn into medCardGroup
                            from mcard in medCardGroup.Where(p => p.MedCenterId == hd.MedCenterId).DefaultIfEmpty()
                            where !string.IsNullOrEmpty(hd.Inn) &&
                                  (sessionStartDateFrom == null || hd.SessionStart >= sessionStartDateFrom) &&
                                  (sessionStartDateTo == null || hd.SessionStart <= sessionStartDateTo.Value.AddDays(1)) &&
                                  (medCenterId == 0 || hd.MedCenterId == medCenterId)
                            select new 
                            {
                                HDSession = hd,
                                Patient = patient,
                                MedCenter = mc,
                                Status = st,
                                MedCenterMachine = mch,
                                MedCard = mcard
                            };

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var searchLower = searchString.ToLower();
                baseQuery = baseQuery.Where(x => 
                    (!string.IsNullOrEmpty(x.HDSession.Inn) && x.HDSession.Inn.ToLower().Contains(searchLower)) ||
                    (x.MedCenter != null && !string.IsNullOrEmpty(x.MedCenter.Title) && 
                     x.MedCenter.Title.ToLower().Contains(searchLower)) ||
                    (!string.IsNullOrEmpty(x.Patient.FirstName) && x.Patient.FirstName.ToLower().Contains(searchLower)) ||
                    (!string.IsNullOrEmpty(x.Patient.LastName) && x.Patient.LastName.ToLower().Contains(searchLower)));
            }
            
            var statusCounts = await baseQuery
                .GroupBy(x => x.HDSession.StatusId)
                .Select(g => new { StatusId = g.Key, Count = g.Count() })
                .ToListAsync();

            
            if (statusId != 0)
            {
                baseQuery = baseQuery.Where(x => x.HDSession.StatusId == statusId);
            }

            baseQuery = sortBy?.ToLower() switch
            {
                "id" => sortAsc ? baseQuery.OrderBy(x => x.HDSession.Id) : baseQuery.OrderByDescending(x => x.HDSession.Id),
                "inn" => sortAsc ? baseQuery.OrderBy(x => x.HDSession.Inn) : baseQuery.OrderByDescending(x => x.HDSession.Inn),
                "sessionstart" => sortAsc ? baseQuery.OrderBy(x => x.HDSession.SessionStart) : baseQuery.OrderByDescending(x => x.HDSession.SessionStart),
                "medcentertitle" => sortAsc ? baseQuery.OrderBy(x => x.MedCenter.Title) : baseQuery.OrderByDescending(x => x.MedCenter.Title),
                "statusname" => sortAsc ? baseQuery.OrderBy(x => x.Status.Title) : baseQuery.OrderByDescending(x => x.Status.Title),
                "patientname" => sortAsc ? baseQuery.OrderBy(x => x.Patient.FirstName).ThenBy(x => x.Patient.LastName) : 
                                          baseQuery.OrderByDescending(x => x.Patient.FirstName).ThenByDescending(x => x.Patient.LastName),
                "startweight" => sortAsc ? baseQuery.OrderBy(x => x.HDSession.StartWeight) : baseQuery.OrderByDescending(x => x.HDSession.StartWeight),
                "endweight" => sortAsc ? baseQuery.OrderBy(x => x.HDSession.EndWeight) : baseQuery.OrderByDescending(x => x.HDSession.EndWeight),
                _ => baseQuery.OrderByDescending(x => x.HDSession.Id) // Varsayılan sıralama
            };

            var totalItems = await baseQuery.CountAsync();

            var sessions = await baseQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var sessionIds = sessions.Select(s => s.HDSession.Id).ToList();
            
            var sessionHours = await _dbContext.HDSessionHour
                .AsNoTracking()
                .Where(h => sessionIds.Contains(h.HDSessionId))
                .ToListAsync();
            
            var sessionPauses = await _dbContext.HDSessionPause
                .AsNoTracking()
                .Where(p => sessionIds.Contains(p.HDSessionId))
                .ToListAsync();

            var hdSessionDtos = sessions.Select(item => new HDSessionDto
            {
                Inn = item.HDSession.Inn,
                HDSession = item.HDSession,
                Patient = item.Patient,
                MedCenter = item.MedCenter,
                Status = item.Status,
                MedCenterMachine = item.MedCenterMachine,
                MedCard = item.MedCard,
                HDSessionHours = sessionHours.Where(h => h.HDSessionId == item.HDSession.Id).ToList(),
                HDSessionPauses = sessionPauses.Where(p => p.HDSessionId == item.HDSession.Id).ToList()
            }).ToList();

            var sessionCount = new SessionCountDto
            {
                All = statusCounts.Sum(x => x.Count), 
                IdentificationCount = statusCounts.FirstOrDefault(x => x.StatusId == (long)HDSessionEnum.Identification)?.Count ?? 0,
                ActiveCount = statusCounts.FirstOrDefault(x => x.StatusId == (long)HDSessionEnum.Started)?.Count ?? 0,
                FinishedCount = statusCounts.FirstOrDefault(x => x.StatusId == (long)HDSessionEnum.Finished)?.Count ?? 0,
                EndIdentificationCount = statusCounts.FirstOrDefault(x => x.StatusId == (long)HDSessionEnum.EndIdentification)?.Count ?? 0,
                SendToPayCount = statusCounts.FirstOrDefault(x => x.StatusId == (long)HDSessionEnum.SendToPay)?.Count ?? 0,
                PayedCount = statusCounts.FirstOrDefault(x => x.StatusId == (long)HDSessionEnum.Payed)?.Count ?? 0,
                OtherCount = statusCounts
                    .Where(x => x.StatusId != (long)HDSessionEnum.Identification &&
                               x.StatusId != (long)HDSessionEnum.Started &&
                               x.StatusId != (long)HDSessionEnum.Finished &&
                               x.StatusId != (long)HDSessionEnum.EndIdentification &&
                               x.StatusId != (long)HDSessionEnum.SendToPay &&
                               x.StatusId != (long)HDSessionEnum.Payed)
                    .Sum(x => x.Count)
            };

            return new SessionResponseDto<HDSessionDto>
            {
                Items = hdSessionDtos,
                TotalItems = totalItems,
                SessionCount = sessionCount,
                IsSuccess = true,
                Message = "Успешно"
            };
        }
        catch (Exception ex)
        {
            // Log the exception here
            return new SessionResponseDto<HDSessionDto>
            {
                Items = new List<HDSessionDto>(),
                TotalItems = 0,
                SessionCount = new SessionCountDto(),
                IsSuccess = false,
                Message = $"Произошла ошибка : {ex.Message}"
            };
        }
    }
}
