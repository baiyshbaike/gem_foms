using System;
using System.Security.Claims;
using Dialysis.Server.Domain;
using Dialysis.Shared.Params;
using Dialysis.Shared.Responses;
using Microsoft.AspNetCore.Identity;
using Dialysis.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Dialysis.Shared.Dto;
using Dialysis.Shared.Constants;
using Dialysis.Client.Pages.Protocols;
using System.Data;
using System.Threading.Tasks;
using MudBlazor;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Drawing;
using DevExpress.CodeParser;
using System.Data.SqlClient;
using Npgsql;
using DocumentFormat.OpenXml.Vml.Office;

namespace Dialysis.Server.Domain.Services
{ public interface IMedCardService
    {
        Task<IRetResult> ImportExcelFile(List<ImportHDSessionsDto> models);
        Task<IRetResult> AddCard(MedCard model);
        Task<IRetResult> AddCardAll(MedCardArgs model);
        Task<Result<List<MedCardListDto>>> SearchCard(string Inn, string LastName, string Name);
        Task<Result<List<MedCardListDto>>> SearchCardByMCenter(string Inn, long Mid);
        Task<Result<List<MedCardListDto>>> AllMedCards();
        Task<Result<MedCardDto>>  MedCardDetail(long medcardId);
        Task<Result<long>> IsAccessCard(long medcardId);
        Task<Result<MedCard>> IsOwnCard(long medcardId);
        Task<Result<HDSession>> GetSessionById(long Id);
        Task<Result<List<HDSessionDto>>> AllIdentify(long medCenterId);
        Task<Result<List<HDSessionDto>>> AllIdentifyAsync();
        Task<IRetResult> AddIdentify(HDSession model);
        Task<IRetResult> EndIdentify(HDSession model);
        Task<Result<HDSession>> GetIdentify(string Inn, long MedCenterId);
        Task<IRetResult> SendToPay(HDSession model);
        Task<IRetResult> DeleteHDSession(HDSession model);
        Task<IRetResult> ArchiveHdSessionById(long id);
        Task<IRetResult> DeleteHDSessionById(long id);
        Task<IRetResult> SendToEnd(HDSession model);
        Task<IRetResult> AddSession(HDSession model);
        Task<IRetResult> EditSession(HDSession model);
        Task<IRetResult> ChekToPay(HDSession model);
        Task<IRetResult> ChekToPayAll(List<HDSession> model);
        Task<IRetResult> StartSession(HDSession model);
        Task<IRetResult> AddSessionStart(HDSession model);
        Task<Result<List<HDSessionDto>>> ByMedCardId(long medcardId, int isCard = 0);
        Task<Result<List<HDSessionDto>>> ActiveSessions(long medCenterId, string isOnly);
        Task<Result<List<HDSessionDto>>> FinishedSessions(long medCenterId, string isOnly);
        Task<Result<List<HDSessionDto>>> AllSessions(long medCenterId, DateTime? fromDate, DateTime? toDate, long? statusId,long? hour);
        Task<IRetResult> AddIndicator(HDSessionHour model);
        Task<IRetResult> UpdateIndicator(HDSessionHour model);
        Task<Result<HDSessionHour>> GetIndicator(long sessionId, string hour);
        Task<IRetResult> AddPause(HDSessionPause model);
        Task<IRetResult> ContinuePause(long sessionId, long pauseId);
        Task<IRetResult> EndSession(HDSession model);
        Task<Result<HDSessionDto>> SessionDetail(long sessionId);
        Task<Result<IndicatorInfoDto>> GetPatientInfo(long medCenterId, DateTime? fromDate, DateTime? toDate);


        Task<IRetResult> AddInspection(FirstInspection model);
        Task<IRetResult> AddRespiratoty(FirstRespiratory model);
        Task<IRetResult> AddCardiovascular(FirstCardiovascular model);
        Task<IRetResult> AddConfectionery(FirstConfectionery model);
        Task<IRetResult> AddUrogenital(FirstUrogenital model);
        Task<IRetResult> AddEndocrine(FirstEndocrine model);
        Task<IRetResult> AddNeuro(FirstNeuro model);
        Task<IRetResult> AddFirstAn(FirstAnalysis model);
        Task<IRetResult> AddEpicrisis(Epicrisis model);

        Task<Result<FirstInspection>> GetInspection(long medcardId);
        Task<Result<FirstRespiratory>> GetRespiratoty(long medcardId);
        Task<Result<FirstCardiovascular>> GetCardiovascular(long medcardId);
        Task<Result<FirstConfectionery>> GetConfectionery(long medcardId);
        Task<Result<FirstAnalysis>> GetFirstAn(long medcardId);
        Task<Result<FirstUrogenital>> GetUrogenital(long medcardId);
        Task<Result<FirstEndocrine>> GetEndocrine(long medcardId);
        Task<Result<FirstNeuro>> GetNeuro(long medcardId);

        Task<Result<Epicrisis>> GetEpicrisis(long medcardId);
        Task<Result<EpicrisisDto>> GetEpicrisisDetail(long Id);
        Task<Result<List<EpicrisisDto>>> GetEpicrisisList(long medcardId);
        Task<IRetResult> AddAnalyses(AnalysisAddArgs model);
        Task<Result<List<AnalysisResultsDto>>> AnalysesByInn(string Inn, DateTime? fromDate, DateTime? toDate);
        Task<Result<List<AnalysisResultsDto>>> AnalysesByIdAndInn(long medCenterId, long analysisId, DateTime? fromDate, DateTime? toDate, decimal fAn, decimal tAn);
        Task<Result<List<AnalysisResultsDto>>> AnalysesByMedCenter(long medCenterId, DateTime? fromDate, DateTime? toDate, decimal fAn, decimal tAn);
        Task<Result<List<AllHDSessionDto>>> AllHDSessions();
        Task<Result<List<AllHDSessionDto>>> AllHDSessionsd(DateTime? fromDate, DateTime? toDate);
        Task<Result<EpicrisisRepDto>> EpicrisisRep(DateTime? fromDate, DateTime? toDate);
        Task<Result<List<PatientSessionsDto>>> GetPatientSessions(string inn);
    }

    public class MedCardService : IMedCardService
    {
        private readonly AppDbContext _dbContext;
        private readonly IActiveUserService _currentUser;
        private readonly IActionLogService _actionLogService;
        private IMedCardService _medCardServiceImplementation;

        public MedCardService(AppDbContext dbContext, IActiveUserService currentUser, IActionLogService actionLogService)
        {
            _dbContext = dbContext;
            _currentUser = currentUser;
            _actionLogService = actionLogService;
        }

        public async Task<IRetResult> ImportExcelFile(List<ImportHDSessionsDto> models)
        {
            foreach (var model in models)
            {
                var isPatient = await _dbContext.Patient.Where(p => p.Inn.Equals(model.HdSession.Inn)).FirstOrDefaultAsync();
                if (isPatient == null)
                {
                    break;
                }
                var medcard = await _dbContext.MedCard.Where(p=>p.PatientId.Equals(isPatient.Id)&& p.MedCenterId.Equals(model.HdSession.MedCenterId)).FirstOrDefaultAsync();
                if (medcard == null)
                {
                    break;
                }

                model.HdSession.ActivePrice = 6500;
                model.HdSession.PatientId = isPatient.Id;
                model.HdSession.MedCardId = medcard.Id;
                model.HdSession.IdentifyBy = _currentUser.UserId;
                model.HdSession.IdentifyOn = DateTime.Now;
                model.HdSession.StatusId = (long)HDSessionEnum.SendToPay;
                model.HdSession.CreatedBy = _currentUser.UserId;
                model.HdSession.CreatedOn = DateTime.Now;
                model.HdSession.TypeMedicine = "test";
                var addnewSession = await _dbContext.AddAsync(model.HdSession);
                await _dbContext.SaveChangesAsync();
                await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "HDSession", addnewSession.Entity.Id.ToString(), null, new { model.HdSession.Inn, StatusId = "SendToPay" }, "Сеанс импортирован из Excel");
                foreach (var item in model.HdSessionHours)
                {
                    item.Title = "test";
                    item.HDSessionId = addnewSession.Entity.Id;
                    item.CreatedBy = _currentUser.UserId;
                    item.CreatedOn = DateTime.Now;
                    var addnew = await _dbContext.AddAsync(item);
                    await _dbContext.SaveChangesAsync();
                    await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "HDSession", addnewSession.Entity.Id.ToString(), null, new { model.HdSession.Inn, StatusId = "SendToPay" }, "Сеанс импортирован из Excel");
                }
            }
            
            return await Result<int>.SuccessAsync();
        }

        public async Task<IRetResult> AddCard(MedCard model)
        {
            if(model != null)
            {
                var oldCard = await _dbContext.MedCard.Where(p => p.PatientId.Equals(model.PatientId) && p.MedCenterId.Equals(model.MedCenterId)).FirstOrDefaultAsync();
                if (model.Id != null && model.Id != 0 && model.CreatedOn != null)
                {
                    //oldCard.PatientId = model.PatientId;
                    oldCard.MedCenterId = model.MedCenterId;//
                    //oldCard.MedCardNumber = model.MedCardNumber;
                    oldCard.Inn = model.Inn;//
                    //oldCard.M3 = model.M3;
                    oldCard.DirectionDate = model.DirectionDate;//
                    oldCard.ReceiptDate = model.ReceiptDate;//
                    //oldCard.LeaveDate = model.LeaveDate;
                    //oldCard.TotalHDSession = model.TotalHDSession;
                    oldCard.Blood = model.Blood;//
                    oldCard.Resus = model.Resus;//
                    oldCard.AllergoAnamez = model.AllergoAnamez;//
                    oldCard.Complication1 = model.Complication1;//
                    oldCard.Complication2 = model.Complication2;//
                    oldCard.Complication3 = model.Complication3;//
                    oldCard.MainDiagnosis = model.MainDiagnosis;//
                    oldCard.Diagnosis1 = model.Diagnosis1;//
                    oldCard.Diagnosis2 = model.Diagnosis2;//
                    oldCard.Diagnosis3 = model.Diagnosis3;//
                    oldCard.OtherAB = model.OtherAB;//
                    oldCard.MainDiagnosisDate = model.MainDiagnosisDate;//
                    oldCard.Complication1Date = model.Complication1Date;//
                    oldCard.Complication2Date = model.Complication2Date;//
                    oldCard.Complication3Date = model.Complication3Date;//
                    oldCard.Diagnosis1Date = model.Diagnosis1Date;//
                    oldCard.Diagnosis3Date = model.Diagnosis3Date;//
                    oldCard.OtherABDate = model.OtherABDate;//
                    oldCard.MainDiagnosisCode = model.MainDiagnosisCode;//
                    oldCard.Complication1Code = model.Complication1Code;//
                    oldCard.Complication2Code = model.Complication2Code;//
                    oldCard.Complication3Code = model.Complication3Code;//
                    oldCard.Diagnosis1Code = model.Diagnosis1Code;//
                    oldCard.Diagnosis2Code = model.Diagnosis2Code;//
                    oldCard.Diagnosis3Code = model.Diagnosis3Code;//
                    oldCard.OtherABCode = model.OtherABCode;//
                    oldCard.OutTreatment = model.OutTreatment;//
                    //oldCard.Title = model.Title;
                    //oldCard.OutcomeTreatment = model.OutcomeTreatment;
                    oldCard.Pedikulez = model.Pedikulez;//
                    oldCard.Chesotka = model.Chesotka;//
                    oldCard.Vassermana = model.Vassermana;//
                    oldCard.Fluorografia = model.Fluorografia;//
                    oldCard.Alchohol = model.Alchohol;//
                    oldCard.Vgv = model.Vgv;//
                    oldCard.VgvDate = model.VgvDate;//
                    oldCard.Vgs = model.Vgs;//
                    oldCard.VgsDate = model.VgsDate;//
                    oldCard.Vich = model.Vich;//
                    oldCard.VichDate = model.VichDate;//
                    //oldCard.Note = model.Note;
                    //oldCard.Obosnovanie = model.Obosnovanie;
                    //oldCard.Plan = model.Plan;
                    //oldCard.IndividualPlan = model.IndividualPlan;
                    //oldCard.Recommendation = model.Recommendation;
                    //oldCard.DoctorId = model.DoctorId;
                    //oldCard.DirectorId = model.DirectorId;
                    oldCard.LastModifiedBy = _currentUser.UserId;
                    oldCard.LastModifiedOn = DateTime.Now;
                    //oldCard.GlobalStatusId = model.GlobalStatusId;
                    //oldCard.ActFile = model.ActFile;
                    //oldCard.UniqId = model.UniqId;
                    oldCard.PassportNum = model.PassportNum;
                    oldCard.ReceiptTime = model.ReceiptTime;
                    oldCard.DirectionTime = model.DirectionTime;
                    oldCard.MainDiagnosisTime = model.MainDiagnosisTime;
                    oldCard.Complication1Time = model.Complication1Time;
                    oldCard.Complication2Time = model.Complication2Time;
                    oldCard.Complication3Time = model.DirectionTime;
                    oldCard.Diagnosis1Time = model.Diagnosis1Time;
                    oldCard.Diagnosis2Time = model.Diagnosis2Time;
                    oldCard.Diagnosis3Time = model.Diagnosis3Time;
                    oldCard.OtherABTime = model.OtherABTime;
                    oldCard.ConductedHemodialysis = model.ConductedHemodialysis;
                    oldCard.Familiarized = model.Familiarized;
                    oldCard.FIODoctor = model.FIODoctor;
                    oldCard.FIODepartmentHead = model.FIODepartmentHead;
                    _dbContext.Attach(oldCard);
                    await _dbContext.SaveChangesAsync();
                    await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "MedCard", oldCard.Id.ToString(), null, new { oldCard.Inn }, "Медицинская карта обновлена");
                }
                else
                {
                    if (oldCard != null)
                    {
                        return await Result<int>.FailAsync("У этого пациента есть мед. карта");
                    }
                    model.CreatedBy = _currentUser.UserId;
                    model.CreatedOn = DateTime.Now;
                    await _dbContext.AddAsync(model);
                    await _dbContext.SaveChangesAsync();
                    await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "MedCard", model.Id.ToString(), null, new { model.Inn, model.PatientId, model.MedCenterId }, "Медицинская карта создана");
                }
            }
            return await Result<int>.SuccessAsync();          
        }

        public async Task<IRetResult> AddCardAll(MedCardArgs model)
        {
            if (model != null)
            {
                var mcard = model.MedCard;
                if (mcard != null)
                {
                    var oldCard = await _dbContext.MedCard.Where(p => p.PatientId.Equals(mcard.PatientId) && p.MedCenterId.Equals(mcard.MedCenterId)).FirstOrDefaultAsync();
                    if (oldCard != null)
                    {
                        return await Result<int>.FailAsync("У этого пациента есть мед. карта");
                    }
                    mcard.CreatedBy = _currentUser.UserId;
                    mcard.CreatedOn = DateTime.Now;
                }
               

                using var transaction = _dbContext.Database.BeginTransaction();
                try
                {
                    var addnew = await _dbContext.AddAsync(mcard);
                    await _dbContext.SaveChangesAsync();
                    var medCardId = addnew.Entity.Id;
                    if (medCardId != null)
                    {
                        model.FirstInspection.MedCardId = medCardId;
                        model.FirstInspection.IsDeleted = false;
                        model.FirstInspection.CreatedBy = _currentUser.UserId;
                        model.FirstInspection.CreatedOn = DateTime.Now;
                        await _dbContext.AddAsync(model.FirstInspection);
                        await _dbContext.SaveChangesAsync();

                        model.FirstRespiratory.MedCardId = medCardId;
                        model.FirstRespiratory.IsDeleted = false;
                        model.FirstRespiratory.CreatedBy = _currentUser.UserId;
                        model.FirstRespiratory.CreatedOn = DateTime.Now;
                        await _dbContext.AddAsync(model.FirstRespiratory);
                        await _dbContext.SaveChangesAsync();

                        model.FirstCardiovascular.MedCardId = medCardId;
                        model.FirstCardiovascular.IsDeleted = false;
                        model.FirstCardiovascular.CreatedBy = _currentUser.UserId;
                        model.FirstCardiovascular.CreatedOn = DateTime.Now;
                        await _dbContext.AddAsync(model.FirstCardiovascular);
                        await _dbContext.SaveChangesAsync();

                        model.FirstConfectionery.MedCardId = medCardId;
                        model.FirstConfectionery.IsDeleted = false;
                        model.FirstConfectionery.CreatedBy = _currentUser.UserId;
                        model.FirstConfectionery.CreatedOn = DateTime.Now;
                        await _dbContext.AddAsync(model.FirstConfectionery);
                        await _dbContext.SaveChangesAsync();

                        model.FirstUrogenital.MedCardId = medCardId;
                        model.FirstUrogenital.IsDeleted = false;
                        model.FirstUrogenital.CreatedBy = _currentUser.UserId;
                        model.FirstUrogenital.CreatedOn = DateTime.Now;
                        await _dbContext.AddAsync(model.FirstUrogenital);
                        await _dbContext.SaveChangesAsync();

                        model.FirstEndocrine.MedCardId = medCardId;
                        model.FirstEndocrine.IsDeleted = false;
                        model.FirstEndocrine.CreatedBy = _currentUser.UserId;
                        model.FirstEndocrine.CreatedOn = DateTime.Now;
                        await _dbContext.AddAsync(model.FirstEndocrine);
                        await _dbContext.SaveChangesAsync();

                        model.FirstNeuro.MedCardId = medCardId;
                        model.FirstNeuro.IsDeleted = false;
                        model.FirstNeuro.CreatedBy = _currentUser.UserId;
                        model.FirstNeuro.CreatedOn = DateTime.Now;
                        await _dbContext.AddAsync(model.FirstNeuro);
                        await _dbContext.SaveChangesAsync();

                        model.FirstAnalysis.MedCardId = medCardId;
                        model.FirstAnalysis.IsDeleted = false;
                        model.FirstAnalysis.CreatedBy = _currentUser.UserId;
                        model.FirstAnalysis.CreatedOn = DateTime.Now;
                        await _dbContext.AddAsync(model.FirstAnalysis);
                        await _dbContext.SaveChangesAsync();

                        transaction.Commit();
                        await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "MedCard", addnew.Entity.Id.ToString(), null, new { mcard.Inn, mcard.PatientId }, "Медицинская карта создана (полностью)");
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


        public async Task<Result<List<MedCardListDto>>> SearchCard(string Inn, string LastName, string Name)
        {
            var relCenters =await  (from u in _dbContext.MedCenter
                          join up in _dbContext.MedCenterUser on u.Id equals up.MedCenterId
                          where up.UserId.Equals(_currentUser.UserId)
                          select u.Id
                          ).ToListAsync();

            if (relCenters != null)
            {
                var allVal = await (from u in _dbContext.MedCard
                                    join up in _dbContext.Patient on u.Inn equals up.Inn
                                    join c in _dbContext.MedCenter on u.MedCenterId equals c.Id
                                    where relCenters.Contains(u.MedCenterId)
                                    && (String.IsNullOrEmpty(Inn) || up.Inn.Equals(Inn))
                                    && (String.IsNullOrEmpty(LastName) || up.LastName.ToLower().Contains(LastName.ToLower()))
                                    && (String.IsNullOrEmpty(Name) || up.FirstName.ToLower().Contains(Name.ToLower()))
                                    //&& 
                                    select new MedCardListDto
                                    {
                                        MedCard = u,
                                        Patient = up,
                                        MedCenter = c
                                    }
                         ).OrderBy(p => p.Patient.LastName).ToListAsync();
                return await Result<List<MedCardListDto>>.SuccessAsync(allVal);
            }
            else
            {
                return await Result<List<MedCardListDto>>.FailAsync("");
            }             
        }

        public async Task<Result<List<MedCardListDto>>> SearchCardByMCenter(string Inn, long Mid)
        {
            
                var allVal = await (from u in _dbContext.MedCard
                                    join up in _dbContext.Patient on u.Inn equals up.Inn
                                    join c in _dbContext.MedCenter on u.MedCenterId equals c.Id
                                    where (Mid==0 || u.MedCenterId.Equals(Mid))
                                    && (String.IsNullOrEmpty(Inn) || up.Inn.Equals(Inn))
                                    select new MedCardListDto
                                    {
                                        MedCard = u,
                                        Patient = up,
                                        MedCenter = c
                                    }
                         ).OrderBy(p => p.Patient.LastName).ToListAsync();
                return await Result<List<MedCardListDto>>.SuccessAsync(allVal);
            
        }

        public async Task<Result<List<MedCardListDto>>> AllMedCards()
        {

            var allVal = await (from u in _dbContext.MedCard
                                join up in _dbContext.Patient on u.Inn equals up.Inn
                                join c in _dbContext.MedCenter on u.MedCenterId equals c.Id
                                select new MedCardListDto
                                {
                                    MedCard = u,
                                    Patient = up,
                                    MedCenter = c
                                }
                     ).OrderByDescending(p => p.MedCard.Id).ToListAsync();
            return await Result<List<MedCardListDto>>.SuccessAsync(allVal);

        }


        public async Task<Result<MedCardDto>> MedCardDetail(long medcardId)
        {
            var allVal = (from u in _dbContext.MedCard
                          join up in _dbContext.Patient on u.Inn equals up.Inn
                          join c in _dbContext.MedCenter on u.MedCenterId equals c.Id
                          where  u.Id.Equals(medcardId)

                          select new MedCardDto
                          {
                              MedCard = u,
                              Patient = up,
                              MedCenter = c
                          }
                          ).FirstOrDefault();
           
                return await Result<MedCardDto>.SuccessAsync(allVal);
        }
        
        public async Task<Result<long>> IsAccessCard(long medcardId)
        {
            var retLong = 0L;
            var allVal = (from u in _dbContext.MedCard
                    join c in _dbContext.MedCenter on u.MedCenterId equals c.Id
                    where  u.Id.Equals(medcardId)
                    select c
                ).FirstOrDefault();
            if (allVal != null)
            {
                var allCenters = await (from u in _dbContext.MedCenter
                        join up in _dbContext.MedCenterUser on u.Id equals up.MedCenterId
                        where up.UserId.Equals(_currentUser.UserId)
                        select u
                    ).OrderBy(p => p.Title).ToListAsync();
                if (allCenters != null)
                {
                    var isOwnCenter = allCenters.Where(p => p.Id.Equals(allVal.Id)).FirstOrDefault();
                    if (isOwnCenter != null)
                    {
                        retLong = isOwnCenter.Id;
                    }
                }
            }
            return await Result<long>.SuccessAsync(retLong);
        }

        public async Task<Result<MedCard>> IsOwnCard(long medcardId)
        {
            var retLong = new MedCard();
            var medCard = await (from u in _dbContext.MedCard                          
                          where u.Id.Equals(medcardId)
                          select u
                ).FirstOrDefaultAsync();

            if (medCard != null)
            {
                var allCenters = await (from u in _dbContext.MedCenter
                                        join up in _dbContext.MedCenterUser on u.Id equals up.MedCenterId
                                        where up.UserId.Equals(_currentUser.UserId)
                                        select u
                    ).OrderBy(p => p.Title).ToListAsync();

                if (allCenters != null)
                {
                    var isOwnCenter = allCenters.Where(p => p.Id.Equals(medCard.MedCenterId)).FirstOrDefault();
                    if (isOwnCenter != null)
                    {
                        retLong = medCard;
                    }
                }
            }
            return await Result<MedCard>.SuccessAsync(retLong);
        }

        public async Task<Result<HDSession>> GetSessionById(long Id)
        {
            var alval =await _dbContext.HDSession.Where(p => p.Id.Equals(Id)).FirstOrDefaultAsync();
            return await Result<HDSession>.SuccessAsync(alval);
        }

        public async Task<Result<List<HDSessionDto>>> ByMedCardId(long medcardId, int isCard=0)
        {
            if (isCard == 0)
            {
                var allVal = await (from u in _dbContext.HDSession
                        join up in _dbContext.Patient on u.Inn equals up.Inn
                        join c in _dbContext.MedCenter on u.MedCenterId equals c.Id
                        join cm in _dbContext.MedCenterMachine on u.MachineId equals cm.Id
                        where (u.StatusId.Equals((long)HDSessionEnum.Finished) || u.StatusId.Equals((long)HDSessionEnum.EndIdentification)  || u.StatusId.Equals((long)HDSessionEnum.SendToPay))
                              && u.MedCardId.Equals(medcardId)
                        select new HDSessionDto
                        {
                            HDSession = u,
                            Patient = up,
                            MedCenter = c,
                            MedCenterMachine = cm
                        }
                    ).OrderByDescending(p => p.HDSession.Id).ToListAsync();

                return await Result<List<HDSessionDto>>.SuccessAsync(allVal);
            }
            else
            {
                var allVal = await (from u in _dbContext.HDSession
                        join up in _dbContext.Patient on u.Inn equals up.Inn
                        join c in _dbContext.MedCenter on u.MedCenterId equals c.Id
                        join cm in _dbContext.MedCenterMachine on u.MachineId equals cm.Id
                        where (u.StatusId.Equals((long)HDSessionEnum.Finished) || u.StatusId.Equals((long)HDSessionEnum.EndIdentification) || u.StatusId.Equals((long)HDSessionEnum.SendToPay))
                                         && u.MedCardId.Equals(medcardId)
                        select new HDSessionDto
                        {
                            HDSession = u,
                            Patient = up,
                            MedCenter = c,
                            MedCenterMachine = cm
                        }
                    ).OrderByDescending(p => p.HDSession.Id).Take(3).ToListAsync();

                return await Result<List<HDSessionDto>>.SuccessAsync(allVal);
            }
        }

        public async Task<Result<List<HDSessionDto>>> ActiveSessions(long medCenterId, string isOnly="0")
        {
            
            var allVal = await (from u in _dbContext.HDSession
                                join up in _dbContext.Patient on u.Inn equals up.Inn
                                join c in _dbContext.MedCenter on u.MedCenterId equals c.Id
                                join cm in _dbContext.MedCenterMachine on u.MachineId equals cm.Id
                                where ( u.StatusId.Equals((long)HDSessionEnum.Started) || u.StatusId.Equals((long)HDSessionEnum.Paused) )
                                && (medCenterId == 0 || c.Id.Equals(medCenterId))
                                select new HDSessionDto
                                {
                                    HDSession = u,
                                    Patient = up,
                                    MedCenter = c,
                                    MedCenterMachine = cm
                                }
                          ).OrderByDescending(p => p.HDSession.Id).ToListAsync();

            if (allVal != null)
            {
                foreach (var item in allVal)
                {
                    item.HDSessionHours =  await _dbContext.HDSessionHour.Where(p => p.HDSessionId.Equals(item.HDSession.Id)).ToListAsync();
                }
            }

            return await Result<List<HDSessionDto>>.SuccessAsync(allVal);
        }

        public async Task<Result<List<HDSessionDto>>> FinishedSessions(long medCenterId, string isOnly = "0")
        {
            var allVal = await (from u in _dbContext.HDSession
                                join up in _dbContext.Patient on u.Inn equals up.Inn
                                join c in _dbContext.MedCenter on u.MedCenterId equals c.Id
                                join cm in _dbContext.MedCenterMachine on u.MachineId equals cm.Id
                                where (u.StatusId.Equals((long)HDSessionEnum.Finished) || u.StatusId.Equals((long)HDSessionEnum.EndIdentification))
                                && (medCenterId == 0 || c.Id.Equals(medCenterId))
                                select new HDSessionDto
                                {
                                    HDSession = u,
                                    Patient = up,
                                    MedCenter = c,
                                    MedCenterMachine = cm
                                }
                          ).OrderByDescending(p => p.HDSession.SessionEnd).ToListAsync();

            if (allVal != null)
            {
                foreach (var item in allVal)
                {
                    item.HDSessionHours = await _dbContext.HDSessionHour.Where(p => p.HDSessionId.Equals(item.HDSession.Id)).ToListAsync();
                }
            }

            return await Result<List<HDSessionDto>>.SuccessAsync(allVal);
        }

        public async Task<Result<List<HDSessionDto>>> AllSessions(long medCenterId, DateTime? fromDate, DateTime? toDate, long? statusId,long? hour)
        {
            toDate = toDate?.AddHours(23).AddMinutes(59);
            var allVal = await (from u in _dbContext.HDSession
                                join up in _dbContext.Patient on u.Inn equals up.Inn
                                join c in _dbContext.MedCenter on u.MedCenterId equals c.Id
                                join cm in _dbContext.MedCenterMachine on u.MachineId equals cm.Id
                                where (u.StatusId.Equals((long)HDSessionEnum.Finished) || u.StatusId.Equals((long)HDSessionEnum.SendToPay) || u.StatusId.Equals((long)HDSessionEnum.Payed) || u.StatusId.Equals((long)HDSessionEnum.Stopped) || u.StatusId.Equals((long)HDSessionEnum.Ended))
                                && (medCenterId == 0 || c.Id.Equals(medCenterId))
                                && (fromDate == null || u.SessionEnd >= fromDate)
                                && (toDate == null || u.SessionEnd <= toDate)
                                && (hour == null || hour == 0 || 
                                    (hour == 1 && u.SessionStart.HasValue && u.SessionEnd.HasValue && (u.SessionEnd.Value - u.SessionStart.Value).TotalHours < 3) ||
                                    (hour == 2 && u.SessionStart.HasValue && u.SessionEnd.HasValue && (u.SessionEnd.Value - u.SessionStart.Value).TotalHours < 4) ||
                                    (hour == 3 && u.SessionStart.HasValue && u.SessionEnd.HasValue && (u.SessionEnd.Value - u.SessionStart.Value).TotalHours >= 4))
                                && (statusId == 0 || u.StatusId.Equals(statusId))
                                select new HDSessionDto
                                {
                                    HDSession = u,
                                    Patient = up,
                                    MedCenter = c,
                                    MedCenterMachine = cm
                                }
                          ).OrderByDescending(p => p.HDSession.Id).ToListAsync();

            

            return await Result<List<HDSessionDto>>.SuccessAsync(allVal);
        }


        public async Task<Result<List<HDSessionDto>>> AllIdentify(long medCenterId)
        {
            var dateBefore = DateTime.Now.AddHours(-4);
            var allVal = await (from h in _dbContext.HDSession
                                join s in _dbContext.Status on h.StatusId equals s.Id
                                join p in _dbContext.Patient on h.Inn equals p.Inn
                                join c in _dbContext.MedCenter on h.MedCenterId equals c.Id
                                join mc in _dbContext.MedCard on new { PatientId = p.Id, MedCenterId = c.Id } equals new { mc.PatientId, mc.MedCenterId } into mCard
                                from mc in mCard.DefaultIfEmpty()
                                where (h.StatusId==(long)HDSessionEnum.Identification && h.IdentifyOn >= dateBefore)
                                && (medCenterId == 0 || c.Id.Equals(medCenterId))
                                /*where (h.StatusId == (long)HDSessionEnum.Identification && h.IdentifyOn >= dateBefore)*/
                                orderby p.LastName
                                select new HDSessionDto
                                {
                                    HDSession = h,
                                    Patient = p,
                                    MedCenter = c,
                                    MedCenterMachine = null,
                                    MedCard = mc,
                                    Status = s
                                }).ToListAsync();
            return await Result<List<HDSessionDto>>.SuccessAsync(allVal);
        }
        public async Task<Result<List<HDSessionDto>>> AllIdentifyAsync()
        {
            /*var dateBefore = DateTime.Now.AddHours(-4);*/
            var allVal = await (from h in _dbContext.HDSession
                                join s in _dbContext.Status on h.StatusId equals s.Id
                                join p in _dbContext.Patient on h.Inn equals p.Inn
                                join c in _dbContext.MedCenter on h.MedCenterId equals c.Id
                                join mc in _dbContext.MedCard on new { PatientId = p.Id, MedCenterId = c.Id } equals new { mc.PatientId, mc.MedCenterId } into mCard
                                from mc in mCard.DefaultIfEmpty()
                                where (h.StatusId == (long)HDSessionEnum.Identification /*&& h.IdentifyOn >= dateBefore*/)
                                orderby p.LastName
                                select new HDSessionDto
                                {
                                    HDSession = h,
                                    Patient = p,
                                    MedCenter = c,
                                    MedCenterMachine = null,
                                    MedCard = mc,
                                    Status = s
                                }).ToListAsync();

            return await Result<List<HDSessionDto>>.SuccessAsync(allVal);
        }
        public async Task<IRetResult> AddIdentify(HDSession model)
        {
            if (model != null)
            {
                var isPatient = await _dbContext.Patient.Where(p => p.Inn.Equals(model.Inn)).FirstOrDefaultAsync();
                if (isPatient == null)
                {
                    return await Result<int>.FailAsync("Нет пациента с этим ИНН/ПИН");
                }

                if (isPatient.GroupId == 2)
                {
                    return await Result<int>.FailAsync("Пациент в архиве");
                }
                if (isPatient.GroupId == 8)
                {
                    return await Result<int>.FailAsync("Это новый пациент");
                }
                DateTime todayDate = DateTime.Today;

                var todaySession = await _dbContext.HDSession.Where(p => 
                    p.Inn.Equals(model.Inn) && 
                    (
                        p.StatusId != (long)HDSessionEnum.Identification ||    
                        (
                            p.StatusId == (long)HDSessionEnum.Identification && 
                            (int)(DateTime.Now - (DateTime)p.IdentifyOn).TotalMinutes < 240
                            )
                        ) &&
                    p.SessionEnd >= todayDate).FirstOrDefaultAsync();
                if (todaySession != null)
                {
                    return await Result<int>.FailAsync("Этот пациент прошел протокол сеанса сегодня.");
                }

                var centerSession = await _dbContext.HDSession.Where(p => (p.Inn.Equals(model.Inn)) && (p.StatusId == (long)HDSessionEnum.Identification)).FirstOrDefaultAsync();
                if (centerSession != null)
                {
                    var dateNow = DateTime.Now;
                    var totalMins = (int)((dateNow - (DateTime)centerSession.IdentifyOn).TotalMinutes);
                    if (totalMins < 240)
                    {
                        return await Result<int>.FailAsync("У этого пациента есть идентификация");
                    }
                    //return await Result<int>.FailAsync("Этот пациент прошел протокол сеанса сегодня.");
                }

                var activeSession = await _dbContext.HDSession.Where(p => p.Inn.Equals(model.Inn) && (p.StatusId.Equals((long)HDSessionEnum.Started) || p.StatusId.Equals((long)HDSessionEnum.Paused))).FirstOrDefaultAsync();
                if (activeSession != null)
                {
                    return await Result<int>.FailAsync("У этого пациента есть активный протокол сеанса");
                }

                model.PatientId = isPatient.Id;
                model.IdentifyBy = _currentUser.UserId;
                model.IdentifyOn = DateTime.Now;
                //model.SessionStart = DateTime.Now;
                model.StatusId = (long)HDSessionEnum.Identification;

                if (!String.IsNullOrEmpty(model.ImageStart))
                {
                    string img_name = isPatient.Id.ToString() + "_" + DateTime.Now.ToString("ddMMyyyyHHmm") + "_start.png";
                    //var imgPath = Path.GetFullPath("wwwroot\\UploadImages\\")+""+img_name;
                    string imgPath = "wwwroot/UploadImages/" + img_name;
                    string base64 = model.ImageStart;
                    base64 = base64.Replace("data:image/jpeg; base64,", "");
                    base64 = base64.Replace("data:image/jpeg;base64,", "");
                    File.WriteAllBytes(imgPath, Convert.FromBase64String(base64));
                    model.ImageStart = img_name;
                }
            }
            var addnew = await _dbContext.AddAsync(model);
            await _dbContext.SaveChangesAsync();
            await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "HDSession", addnew.Entity.Id.ToString(), null, new { model.Inn, model.MedCenterId, StatusId = "Identification" }, "Сеанс идентифицирован");
            return await Result<int>.SuccessAsync();

        }

        public async Task<IRetResult> EndIdentify(HDSession model)
        {
            if (model != null)
            {
                

                var activeSession = await _dbContext.HDSession.Where(p => p.Id.Equals(model.Id)).FirstOrDefaultAsync();
                if (activeSession == null)
                {
                    return await Result<int>.FailAsync("Нет протокола");
                }
                var dateNow = DateTime.Now;
                var totalMins = (int)((dateNow - (DateTime)activeSession.SessionEnd).TotalMinutes);
                
                if (totalMins > 180 && !_currentUser.User.IsInRole("admin"))
                {
                    return await Result<int>.FailAsync("Прошло время идентификации");
                }

                var isPatient = await _dbContext.Patient.Where(p => p.Inn.Equals(model.Inn)).FirstOrDefaultAsync();
                if (isPatient == null)
                {
                    return await Result<int>.FailAsync("Нет пациента с этим ИНН/ПИН");
                }

                //var todayDate = DateTime.Today;
                //var yesterdayDate = todayDate.AddDays(-1);

                var todaySession = await _dbContext.HDSession.Where(p => (p.Inn.Equals(model.Inn)) && (p.StatusId == (long)HDSessionEnum.Finished || p.StatusId == (long)HDSessionEnum.Ended)).FirstOrDefaultAsync();
                if (todaySession == null)
                {
                    return await Result<int>.FailAsync("У этого пациента нет активного протокола сеанса");
                }

                /*var activeSession = await _dbContext.HDSession.Where(p => p.Inn.Equals(model.Inn) && (p.StatusId.Equals((long)HDSessionEnum.Started) || p.StatusId.Equals((long)HDSessionEnum.Paused))).FirstOrDefaultAsync();
                if (activeSession != null)
                {
                    return await Result<int>.FailAsync("У этого пациента есть активный протокол сеанса");
                Прошло время отправки
                }*/



                if (!String.IsNullOrEmpty(model.ImageEnd))
                {
                    string img_name = isPatient.Id.ToString() + "_" + DateTime.Now.ToString("ddMMyyyyHHmm") + "_end.png";
                    //var imgPath = Path.GetFullPath("wwwroot\\UploadImages\\")+""+img_name;
                    string imgPath = "wwwroot/UploadImages/" + img_name;
                    string base64 = model.ImageEnd;
                    base64 = base64.Replace("data:image/jpeg; base64,", "");
                    base64 = base64.Replace("data:image/jpeg;base64,", "");
                    File.WriteAllBytes(imgPath, Convert.FromBase64String(base64));
                    activeSession.ImageEnd = img_name;

                    //model.PatientId = isPatient.Id;
                    activeSession.LastModifiedBy = _currentUser.UserId;
                    activeSession.LastModifiedOn = DateTime.Now;
                    activeSession.StatusId = (long)HDSessionEnum.EndIdentification;
                        _dbContext.Attach(activeSession);
                    await _dbContext.SaveChangesAsync();
                    await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "HDSession", activeSession.Id.ToString(), null, new { StatusId = "EndIdentification" }, "Идентификация завершена");
                }
                else
                {
                    activeSession.ImageEnd = null;

                    //model.PatientId = isPatient.Id;
                    activeSession.LastModifiedBy = _currentUser.UserId;
                    activeSession.LastModifiedOn = DateTime.Now;
                    activeSession.StatusId = (long)HDSessionEnum.EndIdentification;
                    _dbContext.Attach(activeSession);
                    await _dbContext.SaveChangesAsync();
                    await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "HDSession", activeSession.Id.ToString(), null, new { StatusId = "EndIdentification" }, "Идентификация завершена");

                }
            }
            //var addnew = await _dbContext.AddAsync(model);
            //await _dbContext.SaveChangesAsync();
            return await Result<int>.SuccessAsync();
        }



        public async Task<Result<HDSession>> GetIdentify(string Inn, long MedCenterId)
        {
            var allVal = await (from u in _dbContext.HDSession                                
                                where u.Inn == Inn && u.StatusId==(long)HDSessionEnum.Identification && u.MedCenterId==MedCenterId
                                select u
                          ).FirstOrDefaultAsync();
            
            return await Result<HDSession>.SuccessAsync(allVal);
        }

        public async Task<IRetResult> SendToPay(HDSession model)
        {
            if (model != null)
            {

                var dateNow = DateTime.Now;
                var activeSession = await _dbContext.HDSession.Where(p => p.Id.Equals(model.Id)).FirstOrDefaultAsync();
                if (activeSession != null)
                {
                    var totalHours = (int)(((DateTime)model.SessionEnd - (DateTime)activeSession.SessionEnd).TotalHours);

                    if (totalHours > 13 && !_currentUser.User.IsInRole("admin"))
                    {
                        return await Result<int>.FailAsync("Прошло время отправки");
                    }
                    else
                    {

                        activeSession.LastModifiedBy = _currentUser.UserId;
                        activeSession.LastModifiedOn = DateTime.Now;
                        activeSession.StatusId = (long)HDSessionEnum.SendToPay;
                        _dbContext.Attach(activeSession);
                        await _dbContext.SaveChangesAsync();
                        await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "HDSession", activeSession.Id.ToString(), null, new { StatusId = "SendToPay" }, "Отправлено на оплату");
                    }
                }
                else
                {
                    return await Result<int>.FailAsync("У этого пациента нет идентификации");
                }
            }
            else
            {
                return await Result<int>.FailAsync("У этого пациента нет идентификации");
            }

            return await Result<int>.SuccessAsync();
        }
        public async Task<IRetResult> DeleteHDSession(HDSession model)
        {
            if (model != null)
            {

                var dateNow = DateTime.Now;
                var activeSession = await _dbContext.HDSession.Where(p => p.Id.Equals(model.Id)).FirstOrDefaultAsync();
                if (activeSession != null)
                {
                    Console.WriteLine("delete");
                   /* _dbContext.Remove(activeSession);
                    await _dbContext.SaveChangesAsync();*/
                }
                else
                {
                    return await Result<int>.FailAsync("У этого пациента нет идентификации");
                }
            }
            else
            {
                return await Result<int>.FailAsync("У этого пациента нет идентификации");
            }

            return await Result<int>.SuccessAsync();
        }
        public async Task<IRetResult> DeleteHDSessionById(long id)
        {
            if (id != null)
            {

                var dateNow = DateTime.Now;
                var activeSession = await _dbContext.HDSession.Where(p => p.Id.Equals(id)).FirstOrDefaultAsync();
                if (activeSession != null)
                {
                    Console.WriteLine("delete");
                    _dbContext.Remove(activeSession);
                    await _dbContext.SaveChangesAsync();
                    if (activeSession != null)
                        await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Delete", "HDSession", id.ToString(), new { activeSession.Inn }, null, "Сеанс удален");
                }
                else
                {
                    return await Result<int>.FailAsync("У этого пациента нет идентификации");
                }
            }
            else
            {
                return await Result<int>.FailAsync("У этого пациента нет идентификации");
            }

            return await Result<int>.SuccessAsync();
        }
        public async Task<IRetResult> ArchiveHdSessionById(long id)
        {
            if (id != null)
            {

                var dateNow = DateTime.Now;
                var activeSession = await _dbContext.HDSession.Where(p => p.Id.Equals(id)).FirstOrDefaultAsync();
                if (activeSession != null)
                {
                    activeSession.StatusId = (long)HDSessionEnum.Ended;
                    activeSession.LastModifiedOn = DateTime.Now;
                    activeSession.LastModifiedBy = _currentUser.UserId;
                    _dbContext.Update(activeSession);
                    await _dbContext.SaveChangesAsync();
                    await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "HDSession", id.ToString(), null, new { StatusId = "Ended" }, "Сеанс архивирован");
                }
                else
                {
                    return await Result<int>.FailAsync("У этого пациента нет идентификации");
                }
            }
            else
            {
                return await Result<int>.FailAsync("У этого пациента нет идентификации");
            }

            return await Result<int>.SuccessAsync();
        }
        public async Task<IRetResult> ChekToPay(HDSession model)
        {
            if (model != null)
            {

                var dateNow = DateTime.Now;
                var activeSession = await _dbContext.HDSession.Where(p => p.Id.Equals(model.Id)).FirstOrDefaultAsync();
                if (activeSession != null)
                {
                    if (activeSession.StatusId== (long)HDSessionEnum.SendToPay)
                    {
                        activeSession.LastModifiedBy = _currentUser.UserId;
                        activeSession.LastModifiedOn = DateTime.Now;
                        activeSession.StatusId = (long)HDSessionEnum.Payed;
                        _dbContext.Attach(activeSession);
                        await _dbContext.SaveChangesAsync();
                        await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "HDSession", activeSession.Id.ToString(), null, new { StatusId = "Payed" }, "Оплачено");
                    }
                    else
                    {
                        return await Result<int>.FailAsync("Не отправлено на оплату");
                    }
                }
                else
                {
                    return await Result<int>.FailAsync("У этого пациента нет идентификации");
                }
            }
            else
            {
                return await Result<int>.FailAsync("У этого пациента нет идентификации");
            }

            return await Result<int>.SuccessAsync();
        }
        public async Task<IRetResult> ChekToPayAll(List<HDSession> model)
        {
            if (model != null && model.Count > 0)
            {
                var sessionIds = model.Select(m => m.Id).ToList();

                // Toplu güncelleme SQL sorgusu oluşturma
                var idsString = string.Join(",", sessionIds);
                var sql = $@"
                    UPDATE ""HDSession"" 
                    SET ""LastModifiedOn"" = @dateNow, 
                        ""StatusId"" = {(long)HDSessionEnum.Payed} 
                    WHERE ""Id"" IN ({idsString}) 
                      AND ""StatusId"" = {(long)HDSessionEnum.SendToPay}";

                var dateNow = DateTime.Now;

                using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Doğrudan SQL sorgusunu çalıştır
                        var result = await _dbContext.Database.ExecuteSqlRawAsync(sql, new NpgsqlParameter("@dateNow", dateNow));

                        // Başarılıysa değişiklikleri kaydet ve işlemi onayla
                        await transaction.CommitAsync();
                        await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "HDSession", string.Join(",", model.Select(m => m.Id)), null, new { StatusId = "Payed", Count = model.Count }, "Массовая оплата");

                        // Başarı durumu döndür
                        return await Result<int>.SuccessAsync(result);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return await Result<int>.FailAsync(ex.Message);
                    }
                }
            }
            else
            {
                return await Result<int>.FailAsync("У этого пациента нет идентификации");
            }
        }

        public async Task<IRetResult> SendToEnd(HDSession model)
        {
            if (model != null)
            {

                var dateNow = DateTime.Now;
                var activeSession = await _dbContext.HDSession.Where(p => p.Id.Equals(model.Id)).FirstOrDefaultAsync();
                if (activeSession != null)
                {
                    var dateEnd = DateTime.Now;
                    activeSession.SessionEnd = dateEnd;
                    var d = activeSession.SessionEnd.Value.AddMinutes(-30);
                    if (model.SessionEnd != null){
                        if(activeSession.SessionEnd >= model.SessionEnd.Value.AddMinutes(-1))
                        {
                            if(d <= model.SessionEnd)
                            {
                                activeSession.SessionEnd = model.SessionEnd;
                            }
                            else
                            {
                                return await Result<int>.FailAsync("Вы не можете отложить дату закрытия более чем на 30 минут");
                            }
                        }
                        else
                        {
                            return await Result<int>.FailAsync("Дата закрытия должна быть до текущей даты.");
                        }
                    }
                    var totalHours = ((DateTime)activeSession.SessionEnd - (DateTime)activeSession.SessionStart).TotalHours;
                    var totalMinutes = ((DateTime)activeSession.SessionEnd - (DateTime)activeSession.SessionStart).TotalMinutes;

                    var totalPause = 0.0;
                    var totalPauseMinutes = 0.0;

                    var activePrice = await _dbContext.ActivePrice.Where(p => p.IsDeleted.Equals(false)).OrderByDescending(p => p.CreatedOn).FirstOrDefaultAsync();
                    if (activePrice != null) activeSession.ActivePrice = activePrice.Price;

                    var allPauses = await _dbContext.HDSessionPause.Where(p => p.HDSessionId.Equals(activeSession.Id)).ToListAsync();
                    if (allPauses.Any())
                    {
                        foreach (var item in allPauses)
                        {
                            totalPause = totalPause + (((DateTime)item.PauseEnd - (DateTime)item.PauseStart).TotalHours);
                            totalPauseMinutes = totalPauseMinutes + (((DateTime)item.PauseEnd - (DateTime)item.PauseStart).TotalMinutes);
                        }
                    }
                    if (totalPause > 0)
                    {
                        totalHours = totalHours - totalPause;
                        totalMinutes = totalMinutes - totalPauseMinutes;
                    }
                    //activeSession.TotalHours = totalHours;
                    activeSession.TotalHours = MinutsTohHours((int)totalMinutes);
                    activeSession.TotalMinutes = totalMinutes;

                    activeSession.LastModifiedBy = _currentUser.UserId;
                    activeSession.LastModifiedOn = DateTime.Now;
                    
                    activeSession.StatusId = (long)HDSessionEnum.Finished;
                    _dbContext.Attach(activeSession);
                    await _dbContext.SaveChangesAsync();
                    await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "HDSession", activeSession.Id.ToString(), null, new { StatusId = "Finished" }, "Сеанс завершен");
                }               
            }
            return await Result<int>.SuccessAsync();
        }


        public async Task<IRetResult> AddSession(HDSession model)
        {
            if (model != null)
            {
                var activeSession = await _dbContext.HDSession.Where(p => p.PatientId.Equals(model.PatientId) && (p.StatusId.Equals((long)HDSessionEnum.Started) ||  p.StatusId.Equals((long)HDSessionEnum.Paused))).FirstOrDefaultAsync();
                if (activeSession != null)
                {
                    return await Result<int>.FailAsync("У этого пациента есть активный протокол сеанса");
                }
                
                var activeMachine = await _dbContext.HDSession.Where(p => p.MachineId.Equals(model.MachineId) && (p.StatusId.Equals((long)HDSessionEnum.Started) ||  p.StatusId.Equals((long)HDSessionEnum.Paused))).FirstOrDefaultAsync();
                if (activeMachine != null)
                {
                    return await Result<int>.FailAsync("У этого аппарата есть активный протокол сеанса");
                }
                
                var isCardPatient = await _dbContext.MedCard.Where(p => p.Id.Equals(model.MedCardId) && (p.PatientId.Equals(model.PatientId))).FirstOrDefaultAsync();
                if (isCardPatient == null)
                {
                    return await Result<int>.FailAsync("Пациент не принадлежит к этой медицинской карте");
                }
            
                model.CreatedBy = _currentUser.UserId;
                model.CreatedOn = DateTime.Now;
                model.SessionStart = DateTime.Now;
                model.StatusId = (long)HDSessionEnum.Started;

                if (!String.IsNullOrEmpty(model.ImageStart))
                {
                    string img_name = model.PatientId.ToString() + "_" + DateTime.Now.ToString("ddMMyyyyHHmm") + "_start.png";
                    //var imgPath = Path.GetFullPath("wwwroot\\UploadImages\\")+""+img_name;
                    string imgPath = "wwwroot/UploadImages/"+img_name;
                    string base64 = model.ImageStart;
                    base64 = base64.Replace("data:image/jpeg; base64,", "");
                    base64 = base64.Replace("data:image/jpeg;base64,", "");

                    File.WriteAllBytes(imgPath, Convert.FromBase64String(base64));

                    model.ImageStart = img_name;
                }

                var checkMachine = await _dbContext.MedCenterMachine.Where(p => p.Id.Equals(model.MachineId)).FirstOrDefaultAsync();

                var today = DateTime.Today;
                var sessionCountPerDay = await _dbContext.HDSession.CountAsync(p =>
                    p.MachineId == model.MachineId &&
                    p.CreatedOn >= today &&
                    p.StatusId != (long)HDSessionEnum.Identification);

                if (checkMachine != null && checkMachine.TotalSessions.HasValue && sessionCountPerDay >= checkMachine.TotalSessions)
                {
                    return await Result<int>.FailAsync("Аппарат исчерпал лимит сеансов на сегодня");
                }
            }
            var addnew = await _dbContext.AddAsync(model);
            await _dbContext.SaveChangesAsync();
            await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "HDSession", addnew.Entity.Id.ToString(), null, new { model.Inn, model.MedCenterId, StatusId = "Started" }, "Сеанс начат");
            return await Result<int>.SuccessAsync();
        }

        public async Task<IRetResult> EditSession(HDSession model)
        {
            if (model != null)
            {
                var activeSession = await _dbContext.HDSession.Where(p=>p.Id.Equals(model.Id) && (p.StatusId.Equals((long)HDSessionEnum.Started) || p.StatusId.Equals((long)HDSessionEnum.Paused))).FirstOrDefaultAsync();
                if(activeSession != null)
                {
                    if (activeSession?.Сhanged == false)
                    {
                        if (activeSession?.Inn != model.Inn)
                        {   var patient = await _dbContext.Patient.Where(p => p.Inn.Equals(model.Inn)).FirstOrDefaultAsync();
                            var newSession = await _dbContext.HDSession.Where(p => p.Inn.Equals(model.Inn) && p.MedCenterId.Equals(model.MedCenterId) && p.StatusId.Equals((long)HDSessionEnum.Identification)).FirstOrDefaultAsync();
                            if (newSession == null)
                            {
                                return await Result<int>.FailAsync("У этого пациента нет идентификации");
                            }
                            var isCardPatient = await _dbContext.MedCard.Where(p => p.Id.Equals(newSession.MedCardId) && p.MedCenterId.Equals(newSession.MedCardId)).FirstOrDefaultAsync();
                            if (isCardPatient == null)
                            {
                                return await Result<int>.FailAsync("Пациент не принадлежит к этой медицинской карте");
                            }
                            activeSession.Inn = model.Inn;
                            activeSession.MedCardId = isCardPatient.Id;
                            activeSession.PatientId = patient.Id;
                        }

                        if (model.SessionStart == null)
                        {
                            activeSession.Сhanged = true;
                            _dbContext.Update(activeSession);
                            await _dbContext.SaveChangesAsync();
                            await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "HDSession", activeSession.Id.ToString(), null, new { activeSession.Inn, activeSession.SessionStart }, "Сеанс изменен");
                            return await Result<int>.SuccessAsync();
                        }
                        var d = activeSession.SessionStart.Value.AddMinutes(-120);
                        if (activeSession.SessionStart >= model.SessionStart)
                        {
                            if (d <= model.SessionStart)
                            {
                                activeSession.Сhanged = true;
                                activeSession.SessionStart = model.SessionStart;
                                _dbContext.Attach(activeSession);
                                await _dbContext.SaveChangesAsync();
                                await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "HDSession", activeSession.Id.ToString(), null, new { activeSession.Inn, activeSession.SessionStart }, "Сеанс изменен");
                            }
                            else
                            {
                                return await Result<int>.FailAsync("Вы не можете отложить дату начала более чем на 2 часа");
                            }
                        }
                        else
                        {
                            return await Result<int>.FailAsync("Дата начала должна быть до текущей даты.");
                        }
                    }
                    else
                    {
                        return await Result<int>.FailAsync("Пациент был изменен");

                    }
                }
               
                else
                {
                    return await Result<int>.FailAsync("Пациент не идентифицирован");
                }
            }
            else
            {
                return await Result<int>.FailAsync("Пациент не идентифицирован");
            }

            return await Result<int>.SuccessAsync();
        }

        public async Task<IRetResult> StartSession(HDSession model)
        {
            if (model != null)
            {
                var oldSession = await _dbContext.HDSession.Where(p => p.PatientId.Equals(model.PatientId) && (p.StatusId.Equals((long)HDSessionEnum.Started) || p.StatusId.Equals((long)HDSessionEnum.Paused))).FirstOrDefaultAsync();
                if (oldSession != null)
                {
                    return await Result<int>.FailAsync("У этого пациента есть активный протокол сеанса");
                }

                var activeMachine = await _dbContext.HDSession.Where(p => p.MachineId.Equals(model.MachineId) && (p.StatusId.Equals((long)HDSessionEnum.Started) || p.StatusId.Equals((long)HDSessionEnum.Paused))).FirstOrDefaultAsync();
                if (activeMachine !=null)
                {
                    return await Result<int>.FailAsync("У этого аппарата есть активный протокол сеанса");
                }

                var isCardPatient = await _dbContext.MedCard.Where(p => p.Id.Equals(model.MedCardId) && (p.PatientId.Equals(model.PatientId))).FirstOrDefaultAsync();
                if (isCardPatient == null)
                {
                    return await Result<int>.FailAsync("Пациент не принадлежит к этой медицинской карте");
                }
                var checkMachine = await _dbContext.MedCenterMachine.Where(p => p.Id.Equals(model.MachineId)).FirstOrDefaultAsync();

                var today = DateTime.Today;
                var sessionCountPerDay = await _dbContext.HDSession.CountAsync(p =>
                    p.MachineId == model.MachineId &&
                    p.CreatedOn >= today);

                if (checkMachine != null && checkMachine.TotalSessions.HasValue && sessionCountPerDay >= checkMachine.TotalSessions)
                {
                    return await Result<int>.FailAsync("Аппарат исчерпал лимит сеансов на сегодня");
                }
                var activeSession = await _dbContext.HDSession.Where(p => p.Id.Equals(model.Id)).FirstOrDefaultAsync();
                if (activeSession != null)
                {
                    activeSession.CreatedBy = _currentUser.UserId;
                    activeSession.CreatedOn = DateTime.Now;
                    activeSession.StatusId = (long)HDSessionEnum.Started;               
                    activeSession.MachineId = model.MachineId;
                    activeSession.SessionStart = DateTime.Now;
                    activeSession.PatientId = model.PatientId;
                    activeSession.MedCardId = model.MedCardId;
                    _dbContext.Attach(activeSession);
                    await _dbContext.SaveChangesAsync();
                    await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "HDSession", activeSession.Id.ToString(), null, new { StatusId = "Started", activeSession.MachineId }, "Сеанс запущен");
                }
                else
                {
                    return await Result<int>.FailAsync("Пациент не идентифицирован");
                }
            }
            else
            {
                return await Result<int>.FailAsync("Пациент не идентифицирован");
            }

            return await Result<int>.SuccessAsync();
        }
        public async Task<IRetResult> AddSessionStart(HDSession model)
        {
            if (model != null)
            {
                var activeSession = await _dbContext.HDSession.Where(p => p.Id.Equals(model.Id)).FirstOrDefaultAsync();
                if (activeSession != null)
                {
                    activeSession.LastModifiedBy = _currentUser.UserId;
                    activeSession.LastModifiedOn = DateTime.Now;
                    //activeSession.SessionEnd = DateTime.Now;
                    //activeSession.StatusId = (long)HDSessionEnum.Finished;               
                    activeSession.Condition = model.Condition;
                    activeSession.Complaints = model.Complaints;
                    activeSession.Program = model.Program;
                    activeSession.Dialyzer = model.Dialyzer;
                    activeSession.Correction = model.Correction;
                    activeSession.Access = model.Access;
                    activeSession.Anticoagulation = model.Anticoagulation;
                    activeSession.Uf = model.Uf;
                    activeSession.Speed = model.Speed;
                    activeSession.Durators = model.Durators;
                    activeSession.TypeDialyzer = model.TypeDialyzer;
                    activeSession.StartWeight = model.StartWeight;
                    activeSession.Sys = model.Sys;
                    activeSession.Dia = model.Dia;
                    activeSession.Ritm = model.Ritm;
                    activeSession.Temp = model.Temp;
                    activeSession.Reinfusion = model.Reinfusion;


                    _dbContext.Attach(activeSession);
                    await _dbContext.SaveChangesAsync();
                    await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "HDSession", activeSession.Id.ToString(), null, new { activeSession.StartWeight, activeSession.Sys, activeSession.Dia }, "Параметры начала сеанса сохранены");
                }
            }

            return await Result<int>.SuccessAsync();
        }

        public async Task<IRetResult> AddIndicator(HDSessionHour model)
        {
            if (model != null)
            {
                model.CreatedBy = _currentUser.UserId;
                model.CreatedOn = DateTime.Now;
                var addnew = await _dbContext.AddAsync(model);
                await _dbContext.SaveChangesAsync();
                await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "HDSessionHour", addnew.Entity.Id.ToString(), null, new { model.HDSessionId, model.Hour }, "Показатель сеанса добавлен");
            }
            
            return await Result<int>.SuccessAsync();
        }
        public async Task<IRetResult> UpdateIndicator(HDSessionHour model)
        {
            if (model != null)
            {
                var activeSession = await _dbContext.HDSessionHour.Where(p => p.Id.Equals(model.Id)).FirstOrDefaultAsync();
               
                activeSession.Sys = model.Sys;
                activeSession.Dia = model.Dia;
                activeSession.Ritm = model.Ritm;
                activeSession.Temp = model.Temp;
                activeSession.LastModifiedBy = _currentUser.UserId;
                activeSession.LastModifiedOn = DateTime.Now;
                 _dbContext.Attach(activeSession);
                await _dbContext.SaveChangesAsync();
                await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "HDSessionHour", activeSession.Id.ToString(), null, new { activeSession.Sys, activeSession.Dia }, "Показатель сеанса обновлен");
            }

            return await Result<int>.SuccessAsync();
        }

        public async Task<Result<HDSessionHour>> GetIndicator(long sessionId, string hour)
        {
            var hours = await _dbContext.HDSessionHour.Where(p => p.HDSessionId.Equals(sessionId) && p.Hour.Equals(hour)).FirstOrDefaultAsync();
            return await Result<HDSessionHour>.SuccessAsync(hours);
        }

        public async Task<IRetResult> AddPause(HDSessionPause model)
        {
            if (model != null)
            {
                model.CreatedBy = _currentUser.UserId;
                model.CreatedOn = DateTime.Now;
                model.PauseStart = DateTime.Now;
                var addnew = await _dbContext.AddAsync(model);
                await _dbContext.SaveChangesAsync();
                await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "HDSessionPause", addnew.Entity.Id.ToString(), null, new { model.HDSessionId }, "Пауза добавлена");

                var activeSession = await _dbContext.HDSession.Where(p => p.Id.Equals(model.HDSessionId)).FirstOrDefaultAsync();
                if (activeSession != null)
                {
                    activeSession.StatusId = (long)HDSessionEnum.Paused;
                    _dbContext.Attach(activeSession);
                    await _dbContext.SaveChangesAsync();
                    await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "HDSessionPause", addnew.Entity.Id.ToString(), null, new { model.HDSessionId }, "Пауза добавлена");
                }
            }

            return await Result<int>.SuccessAsync();
        }
        public async Task<IRetResult> ContinuePause(long sessionId, long pauseId)
        {
            var activeSession = await _dbContext.HDSession.Where(p => p.Id.Equals(sessionId)).FirstOrDefaultAsync();
            var lastPause = await _dbContext.HDSessionPause.Where(p => p.HDSessionId.Equals(sessionId) && p.PauseEnd.Equals(null)).FirstOrDefaultAsync();
            if (activeSession != null)
            {
                activeSession.StatusId = (long)HDSessionEnum.Started;
                _dbContext.Attach(activeSession);
                await _dbContext.SaveChangesAsync();
            }

            if (lastPause != null)
            {
                lastPause.PauseEnd = DateTime.Now;
                _dbContext.Attach(lastPause);
                await _dbContext.SaveChangesAsync();
            }

            return await Result<int>.SuccessAsync();
        }

        public async Task<IRetResult> EndSession(HDSession model)
        {
            if (model != null)
            {
                var activeSession = await _dbContext.HDSession.Where(p => p.Id.Equals(model.Id)).FirstOrDefaultAsync();

                activeSession.LastModifiedBy = _currentUser.UserId;
                activeSession.LastModifiedOn = DateTime.Now;
                //activeSession.SessionEnd = DateTime.Now;
                //activeSession.StatusId = (long)HDSessionEnum.Finished;

                

                /*if (!String.IsNullOrEmpty(model.ImageEnd))
                {
                    string imgPath = "UploadImages/"+model.PatientId.ToString()+"_"+DateTime.Now.ToString("ddMMyyyyHHmm")+"_end.png";
                    string base64 = model.ImageEnd;
                    base64 = base64.Replace("data:image/jpeg; base64,", "");
                    base64 = base64.Replace("data:image/jpeg;base64,", "");

                    File.WriteAllBytes(imgPath, Convert.FromBase64String(base64));

                    activeSession.ImageEnd = imgPath;
                }*/
                activeSession.EndDia = model.EndDia;
                activeSession.EndRitm = model.EndRitm;
                activeSession.EndSys = model.EndSys;
                activeSession.EndTemp = model.EndTemp;
                activeSession.EndWeight = model.EndWeight;
                activeSession.Hd1Hypotension = model.Hd1Hypotension;
                activeSession.Hd2Hypertension = model.Hd2Hypertension;
                activeSession.Hd3MuscleCrampsOfTheLimbs = model.Hd3MuscleCrampsOfTheLimbs;
                activeSession.Hd4HeartRhythmDisturbances = model.Hd4HeartRhythmDisturbances;
                activeSession.Hd5Headache = model.Hd5Headache;
                activeSession.Hd6AnginaAttacks = model.Hd6AnginaAttacks;
                activeSession.Hd7Other = model.Hd7Other;
                activeSession.Hd8CorrectionOfComplications = model.Hd8CorrectionOfComplications;
                activeSession.Hd9PlannedAppointments = model.Hd9PlannedAppointments;
                activeSession.Hd10Recommendations = model.Hd10Recommendations;
                activeSession.Hd11EffectiveTime = model.Hd11EffectiveTime;
                activeSession.Note2 = model.Note2;
                activeSession.Note3 = model.Note3;
                activeSession.FioDoctor = model.FioDoctor;
                activeSession.FioDepartmentHead = model.FioDepartmentHead;
                //activeSession.StatusId = (long)HDSessionEnum.Finished;

                _dbContext.Attach(activeSession);
                await _dbContext.SaveChangesAsync();
                await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "HDSession", activeSession.Id.ToString(), null, new { activeSession.EndWeight, activeSession.EndSys, activeSession.EndDia }, "Параметры окончания сеанса сохранены");
            }
                  
            return await Result<int>.SuccessAsync();
        }
        
        private  double MinutsTohHours(int totalMinutes)
        {
            double totalHours = 0;
            totalHours = ((double)(totalMinutes % 60) / 100);
            return Math.Floor((double)totalMinutes / 60) + totalHours;
        }

        public async Task<Result<HDSessionDto>> SessionDetail(long sessionId)
        {
            var allVal = await (from u in _dbContext.HDSession
                                join up in _dbContext.Patient on u.Inn equals up.Inn
                                join c in _dbContext.MedCenter on u.MedCenterId equals c.Id
                                join cm in _dbContext.MedCenterMachine on u.MachineId equals cm.Id
                                where u.Id.Equals(sessionId)
                                select new HDSessionDto
                                {
                                    HDSession = u,
                                    Patient = up,
                                    MedCenter = c,
                                    MedCenterMachine = cm
                                }
                          ).FirstOrDefaultAsync();
            if (allVal != null)
            {
                allVal.HDSessionHours = await _dbContext.HDSessionHour.Where(p => p.HDSessionId.Equals(sessionId)).OrderBy(p => p.Hour).ToListAsync();
                allVal.HDSessionPauses = await _dbContext.HDSessionPause.Where(p => p.HDSessionId.Equals(sessionId)).OrderBy(p => p.Id).ToListAsync();

            }
            return await Result<HDSessionDto>.SuccessAsync(allVal);
        }
        public async Task<Result<IndicatorInfoDto>> GetPatientInfo(long medCenterId, DateTime? fromDate, DateTime? toDate)
        {
            int dayDifference = (int)(toDate.Value - fromDate.Value).TotalDays;
            var sessionSize = (int)(dayDifference / 7)*3;
            var allVal = await (from u in _dbContext.HDSession
                                join up in _dbContext.Patient on u.Inn equals up.Inn
                                join c in _dbContext.MedCenter on u.MedCenterId equals c.Id
                                where (fromDate == null || u.SessionStart >= fromDate)
                                      && (toDate == null || u.SessionEnd <= toDate)
                                      && (medCenterId == 0 || u.MedCenterId.Equals(medCenterId))
                                select up
                          ).ToListAsync();
            var allVal2 = await (from u in _dbContext.HDSession
                    join up in _dbContext.Patient on u.Inn equals up.Inn
                    join c in _dbContext.MedCenter on u.MedCenterId equals c.Id
                    where (fromDate == null || u.SessionStart >= fromDate)
                          && (toDate == null || u.SessionEnd <= toDate)
                          && (medCenterId == 0 || u.MedCenterId.Equals(medCenterId))
                    select u
                ).ToListAsync();
            List<long> patientsOfWeekIds = new List<long>();
            if (allVal2 != null)
            {
                var patientCounts = allVal2.GroupBy(u => u.PatientId)
                    .ToDictionary(g => g.Key, g => g.Count());

                foreach (var kvp in patientCounts)
                {
                    if (kvp.Value > sessionSize)
                    {
                        patientsOfWeekIds.Add(kvp.Key);
                    }
                }
            }

            List<Patient> patientList = new List<Patient>();
            foreach (var id in patientsOfWeekIds)
            {
                var pat = _dbContext.Patient.Where(p => p.Id.Equals(id)).FirstOrDefault();
                if (pat != null)
                {
                    patientList.Add(pat);
                }
            }
            IndicatorInfoDto item = new IndicatorInfoDto()
            {
                Patients = allVal,
                PatientsOfWeek = patientList
            };
            return await Result<IndicatorInfoDto>.SuccessAsync(item);
        }


        #region StartMedCardExtra
        public async Task<IRetResult> AddInspection(FirstInspection model)
        {
            if (model != null && model.Id==0)
            {
                model.CreatedBy = _currentUser.UserId;
                model.CreatedOn = DateTime.Now;
                await _dbContext.AddAsync(model);
                await _dbContext.SaveChangesAsync();
                await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "FirstInspection", model.Id.ToString(), null, new { model.MedCardId }, "Осмотр сохранен");
            }
            else if(model !=null && model.Id >0)
            {
                model.LastModifiedBy = _currentUser.UserId;
                model.LastModifiedOn = DateTime.Now;
                _dbContext.Update(model);
                await _dbContext.SaveChangesAsync();
                await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "FirstInspection", model.Id.ToString(), null, new { model.MedCardId }, "Осмотр сохранен");
            }
            return await Result<int>.SuccessAsync();
        }
        public async Task<IRetResult> AddRespiratoty(FirstRespiratory model)
        {
            if (model != null)
            {
                if(model.Id > 0)
                {
                    model.LastModifiedBy = _currentUser.UserId;
                    model.LastModifiedOn = DateTime.Now;
                    _dbContext.Update(model);
                    await _dbContext.SaveChangesAsync();
                    await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "FirstRespiratory", model.Id.ToString(), null, new { model.MedCardId }, "Осмотр сохранен");
                }
                else
                {
                    model.CreatedBy = _currentUser.UserId;
                    model.CreatedOn = DateTime.Now;
                    model.IsDeleted = false;
                    await _dbContext.AddAsync(model);
                    await _dbContext.SaveChangesAsync();
                    await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "FirstRespiratory", model.Id.ToString(), null, new { model.MedCardId }, "Осмотр сохранен");
                }
            }
            return await Result<int>.SuccessAsync();
        }
        public async Task<IRetResult> AddCardiovascular(FirstCardiovascular model)
        {
            if (model != null)
            {
                if (model.Id >=0)
                {
                    model.LastModifiedBy = _currentUser.UserId;
                    model.LastModifiedOn = DateTime.Now;
                    _dbContext.Update(model);
                    await _dbContext.SaveChangesAsync();
                    await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "FirstCardiovascular", model.Id.ToString(), null, new { model.MedCardId }, "Осмотр сохранен");
                }
                else
                {
                    model.CreatedBy = _currentUser.UserId;
                    model.CreatedOn = DateTime.Now;
                    model.IsDeleted = false;
                    await _dbContext.AddAsync(model);
                    await _dbContext.SaveChangesAsync();
                    await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "FirstCardiovascular", model.Id.ToString(), null, new { model.MedCardId }, "Осмотр сохранен");
                }
            }
            return await Result<int>.SuccessAsync();
        }

        public async Task<IRetResult> AddFirstAn(FirstAnalysis model)
        {
            if (model != null)
            {
                if (model.Id >= 0)
                {
                    model.LastModifiedBy = _currentUser.UserId;
                    model.LastModifiedOn = DateTime.Now;
                    _dbContext.Update(model);
                    await _dbContext.SaveChangesAsync();
                    await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "FirstAnalysis", model.Id.ToString(), null, new { model.MedCardId }, "Осмотр сохранен");
                }
                else
                {
                    model.CreatedBy = _currentUser.UserId;
                    model.CreatedOn = DateTime.Now;
                    model.IsDeleted = false;
                    await _dbContext.AddAsync(model);
                    await _dbContext.SaveChangesAsync();
                    await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "FirstAnalysis", model.Id.ToString(), null, new { model.MedCardId }, "Осмотр сохранен");
                }
            }
            return await Result<int>.SuccessAsync();
        }

        public async Task<IRetResult> AddConfectionery(FirstConfectionery model)
        {
            if (model != null)
            {
                if (model.Id >= 0)
                {
                    model.LastModifiedBy = _currentUser.UserId;
                    model.LastModifiedOn = DateTime.Now;
                    _dbContext.Update(model);
                    await _dbContext.SaveChangesAsync();
                    await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "FirstConfectionery", model.Id.ToString(), null, new { model.MedCardId }, "Осмотр сохранен");
                }
                else
                {
                    model.CreatedBy = _currentUser.UserId;
                    model.CreatedOn = DateTime.Now;
                    model.IsDeleted = false;
                    await _dbContext.AddAsync(model);
                    await _dbContext.SaveChangesAsync();
                    await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "FirstConfectionery", model.Id.ToString(), null, new { model.MedCardId }, "Осмотр сохранен");
                }
            }
            return await Result<int>.SuccessAsync();
        }
        public async Task<IRetResult> AddUrogenital(FirstUrogenital model)
        {
            if (model != null)
            {
                if (model.Id >= 0)
                {
                    model.LastModifiedBy = _currentUser.UserId;
                    model.LastModifiedOn = DateTime.Now;
                    _dbContext.Update(model);
                    await _dbContext.SaveChangesAsync();
                    await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "FirstUrogenital", model.Id.ToString(), null, new { model.MedCardId }, "Осмотр сохранен");
                }
                else
                {
                    model.CreatedBy = _currentUser.UserId;
                    model.CreatedOn = DateTime.Now;
                    model.IsDeleted = false;
                    await _dbContext.AddAsync(model);
                    await _dbContext.SaveChangesAsync();
                    await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "FirstUrogenital", model.Id.ToString(), null, new { model.MedCardId }, "Осмотр сохранен");
                }
            }
            return await Result<int>.SuccessAsync();
        }
        public async Task<IRetResult> AddEndocrine(FirstEndocrine model)
        {
            if (model != null)
            {
                if (model.Id >= 0)
                {
                    model.LastModifiedBy = _currentUser.UserId;
                    model.LastModifiedOn = DateTime.Now;
                    _dbContext.Update(model);
                    await _dbContext.SaveChangesAsync();
                    await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "FirstEndocrine", model.Id.ToString(), null, new { model.MedCardId }, "Осмотр сохранен");
                }
                else
                {
                    model.CreatedBy = _currentUser.UserId;
                    model.CreatedOn = DateTime.Now;
                    model.IsDeleted = false;
                    await _dbContext.AddAsync(model);
                    await _dbContext.SaveChangesAsync();
                    await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "FirstEndocrine", model.Id.ToString(), null, new { model.MedCardId }, "Осмотр сохранен");
                }
            }
            return await Result<int>.SuccessAsync();
        }
        public async Task<IRetResult> AddNeuro(FirstNeuro model)
        {
            if (model != null)
            {
                if (model.Id >= 0)
                {
                    model.LastModifiedBy = _currentUser.UserId;
                    model.LastModifiedOn = DateTime.Now;
                    _dbContext.Update(model);
                    await _dbContext.SaveChangesAsync();
                    await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "FirstNeuro", model.Id.ToString(), null, new { model.MedCardId }, "Осмотр сохранен");
                }
                else
                {
                    model.CreatedBy = _currentUser.UserId;
                    model.CreatedOn = DateTime.Now;
                    model.IsDeleted = false;
                    await _dbContext.AddAsync(model);
                    await _dbContext.SaveChangesAsync();
                    await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "FirstNeuro", model.Id.ToString(), null, new { model.MedCardId }, "Осмотр сохранен");
                }
            }
            return await Result<int>.SuccessAsync();
        }
        public async Task<IRetResult> AddEpicrisis(Epicrisis model)
        {
            if (model != null)
            {


                if (model.Id > 0)
                {
                    model.LastModifiedBy = _currentUser.UserId;
                    model.LastModifiedOn = DateTime.Now;
                    _dbContext.Update(model);
                    await _dbContext.SaveChangesAsync();
                    await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Update", "Epicrisis", model.Id.ToString(), null, new { model.MedCardId }, "Осмотр сохранен");
                }
                else
                {
                    var selPatient = await (from u in _dbContext.Patient
                                            join up in _dbContext.MedCard on u.Id equals up.PatientId
                                            where up.Id.Equals(model.MedCardId)
                                            select u
                    ).FirstOrDefaultAsync();
                    if (selPatient != null)
                    {
                        model.PatientId = selPatient.Id;
                    }

                    model.CreatedBy = _currentUser.UserId;
                    model.CreatedOn = DateTime.Now;
                    await _dbContext.AddAsync(model);
                    await _dbContext.SaveChangesAsync();
                    await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "Epicrisis", model.Id.ToString(), null, new { model.MedCardId }, "Осмотр сохранен");
                }
            }
            return await Result<int>.SuccessAsync();

            /*if (model != null)
            {
                var selPatient = await (from u in _dbContext.Patient
                        join up in _dbContext.MedCard on u.Id equals up.PatientId
                        where up.Id.Equals(model.MedCardId)
                        select u
                    ).FirstOrDefaultAsync();
                if (selPatient != null)
                {
                    model.PatientId = selPatient.Id;
                }
                model.CreatedBy = _currentUser.UserId;
                model.CreatedOn = DateTime.Now;
                await _dbContext.AddAsync(model);
                await _dbContext.SaveChangesAsync();
            }
            return await Result<int>.SuccessAsync();*/
        }
        /*public async Task<IRetResult> AddAnalysis(FirstAnalysis model)
        {
            if (model != null)
            {
                model.CreatedBy = _currentUser.UserId;
                model.CreatedOn = DateTime.Now;
                await _dbContext.AddAsync(model);
                await _dbContext.SaveChangesAsync();
            }
            return await Result<int>.SuccessAsync();
        }*/

        public async Task<Result<FirstAnalysis>> GetFirstAn(long medcardId)
        {
            var allVal = await _dbContext.FirstAnalysis.Where(p => p.MedCardId.Equals(medcardId)).FirstOrDefaultAsync();
            return await Result<FirstAnalysis>.SuccessAsync(allVal);
        }

        public async Task<Result<FirstInspection>> GetInspection(long medcardId)
        {
            var allVal = await _dbContext.FirstInspection.Where(p => p.MedCardId.Equals(medcardId)).FirstOrDefaultAsync();
            return await Result<FirstInspection>.SuccessAsync(allVal);
        }
        public async Task<Result<FirstRespiratory>> GetRespiratoty(long medcardId)
        {
            var allVal = await _dbContext.FirstRespiratory.Where(p => p.MedCardId.Equals(medcardId)).FirstOrDefaultAsync();
            return await Result<FirstRespiratory>.SuccessAsync(allVal);
        }
        public async Task<Result<FirstCardiovascular>> GetCardiovascular(long medcardId)
        {
            var allVal = await _dbContext.FirstCardiovascular.Where(p => p.MedCardId.Equals(medcardId)).FirstOrDefaultAsync();
            return await Result<FirstCardiovascular>.SuccessAsync(allVal);
        }

        public async Task<Result<FirstConfectionery>> GetConfectionery(long medcardId)
        {
            var allVal = await _dbContext.FirstConfectionery.Where(p => p.MedCardId.Equals(medcardId)).FirstOrDefaultAsync();
            return await Result<FirstConfectionery>.SuccessAsync(allVal);
        }

        public async Task<Result<FirstUrogenital>> GetUrogenital(long medcardId)
        {
            var allVal = await _dbContext.FirstUrogenital.Where(p => p.MedCardId.Equals(medcardId)).FirstOrDefaultAsync();
            return await Result<FirstUrogenital>.SuccessAsync(allVal);
        }

        public async Task<Result<FirstEndocrine>> GetEndocrine(long medcardId)
        {
            var allVal = await _dbContext.FirstEndocrine.Where(p => p.MedCardId.Equals(medcardId)).FirstOrDefaultAsync();
            return await Result<FirstEndocrine>.SuccessAsync(allVal);
        }

        public async Task<Result<FirstNeuro>> GetNeuro(long medcardId)
        {
            var allVal = await _dbContext.FirstNeuro.Where(p => p.MedCardId.Equals(medcardId)).FirstOrDefaultAsync();
            return await Result<FirstNeuro>.SuccessAsync(allVal);
        }
        
        public async Task<Result<Epicrisis>> GetEpicrisis(long medcardId)
        {
            var allVal = await _dbContext.Epicrisis.Where(p => p.MedCardId.Equals(medcardId)).FirstOrDefaultAsync();
            return await Result<Epicrisis>.SuccessAsync(allVal);
        }
        public async Task<Result<EpicrisisDto>> GetEpicrisisDetail(long Id)
        {
            var allVal = await (from u in _dbContext.Epicrisis
                    join up in _dbContext.Patient on u.PatientId equals up.Id
                    join m in _dbContext.MedCard on u.MedCardId equals m.Id
                    join c in _dbContext.MedCenter on m.MedCenterId equals c.Id
                    where u.Id.Equals(Id)
                    select new EpicrisisDto
                    {
                        Epicrisis = u,
                        Patient = up,
                    }
                ).FirstOrDefaultAsync();
            //var allVal = await _dbContext.Epicrisis.Where(p => p.Id.Equals(Id)).FirstOrDefaultAsync();
            return await Result<EpicrisisDto>.SuccessAsync(allVal);
        }
        
        public async Task<Result<List<EpicrisisDto>>> GetEpicrisisList(long medcardId)
        {
            var allVal = await (from u in _dbContext.Epicrisis
                    join up in _dbContext.Patient on u.PatientId equals up.Id
                    where (u.MedCardId.Equals(medcardId))
                    select new EpicrisisDto
                    {
                        Epicrisis = u,
                        Patient = up,
                    }
                ).OrderByDescending(p=>p.Epicrisis.Id).ToListAsync();
            
            return await Result<List<EpicrisisDto>>.SuccessAsync(allVal);
        }

        #endregion

        public async Task<IRetResult> AddAnalyses(AnalysisAddArgs model)
        {
            if (model != null)
            {
                foreach (var item in model.AnalysisResults)
                {
                    if (item != null)
                    {
                        AnalysisResult addModel = item;
                        addModel.CreatedBy = _currentUser.UserId;
                        addModel.CreatedOn = DateTime.Now;                       
                        var addnew = await _dbContext.AddAsync(addModel);
                        await _dbContext.SaveChangesAsync();
                        await _actionLogService.LogAsync(_currentUser.UserId, _currentUser.User.FindFirstValue(ClaimTypes.Name) ?? _currentUser.User.FindFirstValue(ClaimTypes.GivenName), _currentUser.UserIp, "Create", "AnalysisResult", addnew.Entity.Id.ToString(), null, new { addModel.AnalysisId, addModel.Inn }, "Результат анализа добавлен");
                    }
                }
            }            
            return await Result<int>.SuccessAsync();
        }

        public async Task<Result<List<AnalysisResultsDto>>> AnalysesByInn(string Inn, DateTime? fromDate, DateTime? toDate)
        {
            var userId = _currentUser.UserId;
            var medcenters = (from u in _dbContext.MedCenter
                    join up in _dbContext.MedCenterUser on u.Id equals up.MedCenterId
                    where up.UserId.Equals(userId)
                    select u
                ).OrderBy(p => p.Title).ToList();
            var IsOnlyCenter = medcenters.Count==1;
            var allVal = await (from u in _dbContext.AnalysisResult
                                join up in _dbContext.Analysise on u.AnalysisId equals up.Id
                                join p in _dbContext.Patient on u.Inn equals p.Inn
                                join m in _dbContext.MedCard on u.MedCardId equals m.Id
                                where (p.Inn.Equals(Inn))
                                && (fromDate == null || u.AnalysisDate >= fromDate)
                                && (toDate == null || u.AnalysisDate <= toDate)
                                && (!IsOnlyCenter || m.MedCenterId == medcenters[0].Id)
                                select new AnalysisResultsDto
                                {
                                    Analysis = up,
                                    AnalysisResult = u,
                                    Patient = p,
                                    MedCard = m
                                }
                          ).OrderByDescending(p => p.AnalysisResult.CreatedOn).ToListAsync();

            return await Result<List<AnalysisResultsDto>>.SuccessAsync(allVal);
        }


        public async Task<Result<List<AnalysisResultsDto>>> AnalysesByIdAndInn(long medCenterId, long analysisId, DateTime? fromDate, DateTime? toDate, decimal fAn, decimal tAn)
        {
            var retVal = new List<AnalysisResultsDto>();
            
                var allVal = await (from u in _dbContext.AnalysisResult
                                    join up in _dbContext.Analysise on u.AnalysisId equals up.Id
                                    join p in _dbContext.Patient on u.Inn equals p.Inn
                                    join m in _dbContext.MedCard on u.MedCardId equals m.Id
                                    where (analysisId == 0 || u.AnalysisId.Equals(analysisId))
                                    && (fromDate == null || u.AnalysisDate >= fromDate)
                                    && (toDate == null || u.AnalysisDate <= toDate)
                                    && (medCenterId == 0 || m.MedCenterId.Equals(medCenterId))
                                    select new AnalysisResultsDto
                                    {
                                        Analysis = up,
                                        AnalysisResult = u,
                                        Patient = p,
                                        MedCard = m
                                    }
                              ).OrderByDescending(p => p.AnalysisResult.CreatedOn).ToListAsync();

                if (allVal != null && analysisId > 0)
                {
                    foreach (var item in allVal)
                    {
                        if (fAn > 0 && tAn > 0)
                        {
                            try
                            {
                                if (Convert.ToDecimal(item.AnalysisResult.Result) >= fAn && Convert.ToDecimal(item.AnalysisResult.Result) <= tAn)
                                {
                                    retVal.Add(item);
                                }
                            }
                            catch (Exception)
                            {

                            }
                        }
                        else if (fAn > 0 && tAn == 0)
                        {
                            try
                            {
                                if (Convert.ToDecimal(item.AnalysisResult.Result) >= fAn)
                                {
                                    retVal.Add(item);
                                }
                            }
                            catch (Exception)
                            {

                            }
                        }
                        else if (fAn == 0 && tAn < 0)
                        {
                            try
                            {
                                if (Convert.ToDecimal(item.AnalysisResult.Result) <= tAn)
                                {
                                    retVal.Add(item);
                                }
                            }
                            catch (Exception)
                            {

                            }
                        }
                        else
                        {
                            retVal.Add(item);
                        }
                    }
                }
                else
                {
                    retVal = allVal;
                }
            return await Result<List<AnalysisResultsDto>>.SuccessAsync(retVal);
        }


        public async Task<Result<List<AnalysisResultsDto>>> AnalysesByMedCenter(long medCenterId, DateTime? fromDate, DateTime? toDate, decimal fAn, decimal tAn)
        {
            var retVal = new List<AnalysisResultsDto>();
            var allVal = await (from u in _dbContext.AnalysisResult
                                join up in _dbContext.Analysise on u.AnalysisId equals up.Id
                                join p in _dbContext.Patient on u.Inn equals p.Inn
                                join m in _dbContext.MedCard on u.MedCardId equals m.Id
                                join med in _dbContext.MedCenter on m.MedCenterId equals med.Id
                                where (fromDate == null || u.AnalysisDate >= fromDate)
                                      && (toDate == null || u.AnalysisDate <= toDate)
                                      && (medCenterId == 0 || m.MedCenterId.Equals(medCenterId))
                                select new AnalysisResultsDto
                                {
                                    Analysis = up,
                                    AnalysisResult = u,
                                    Patient = p,
                                    MedCard = m,
                                    MedCenter = med
                                }
                          ).OrderByDescending(p => p.AnalysisResult.CreatedOn).ToListAsync();
            if (allVal != null)
            {
                foreach (var item in allVal)
                {
                    if (fAn > 0 && tAn > 0)
                    {
                        try
                        {
                            if (Convert.ToDecimal(item.AnalysisResult.Result) >= fAn && Convert.ToDecimal(item.AnalysisResult.Result) <= tAn)
                            {
                                retVal.Add(item);
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                    else if (fAn > 0 && tAn == 0)
                    {
                        try
                        {
                            if (Convert.ToDecimal(item.AnalysisResult.Result) >= fAn)
                            {
                                retVal.Add(item);
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                    else if (fAn == 0 && tAn > 0)
                    {
                        try
                        {
                            if (Convert.ToDecimal(item.AnalysisResult.Result) <= tAn)
                            {
                                retVal.Add(item);
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                    else
                    {
                        retVal.Add(item);
                    }
                }
            }
            else
            {
                retVal = allVal;
            }
            return await Result<List<AnalysisResultsDto>>.SuccessAsync(retVal);
        }
        public async Task<Result<List<AllHDSessionDto>>> AllHDSessions()
        {
            var allVal = await (from u in _dbContext.HDSession
                                join up in _dbContext.Patient on u.Inn equals up.Inn
                                join c in _dbContext.MedCenter on u.MedCenterId equals c.Id
                                join cm in _dbContext.MedCenterMachine on u.MachineId equals cm.Id
                                join reg in _dbContext.Region on c.RegionId equals reg.Id
                                join dist in _dbContext.District on c.DistrictId equals dist.Id    
                                where (u.StatusId.Equals((long)HDSessionEnum.SendToPay) || u.StatusId.Equals((long)HDSessionEnum.Finished) || u.StatusId.Equals((long)HDSessionEnum.Started) || u.StatusId.Equals((long)HDSessionEnum.Paused))                               
                                select new AllHDSessionDto
                                {
                                    Id = u.Id,
                                    Inn = u.Inn,
                                    Fio = up.LastName+" "+up.FirstName+ " "+up.MiddleName!,
                                    MedCenterTitle = c.Title,
                                    District = dist.Title,
                                    Region = reg.Title,
                                    Age = u.Inn,
                                    MachineTitle = cm.Number+" "+cm.Name+" "+cm.Model,
                                    StatusTitle = "",
                                    GenderTitle =(up.Gender==true?"Муж":"Жен"),
                                    Condition = u.Condition,
                                    Complaints = u.Complaints,
                                    Program = u.Program,
                                    Dialyzer = u.Dialyzer,
                                    Correction = u.Correction,
                                    Reinfusion = u.Reinfusion,
                                    Access = u.Access,
                                    Anticoagulation = u.Anticoagulation,
                                    Uf = u.Uf,
                                    Speed = u.Speed,
                                    TypeDialyzer = u.TypeDialyzer,
                                    Durators = u.Durators,
                                    StartWeight = u.StartWeight,
                                    EndWeight = u.EndWeight,
                                    Note = u.Note,
                                    SessionStart = u.SessionStart,
                                    SessionEnd = u.SessionEnd,
                                    StatusId = u.StatusId,
                                    TotalMinutes = u.TotalMinutes,
                                    TotalHours = u.TotalHours,
                                    ActivePrice = u.ActivePrice,
                                    SetPrice = u.SetPrice,
                                    Note2 = u.Note2,
                                    Note3 = u.Note3,
                                    ImageStart = u.ImageStart,
                                    ImageEnd = u.ImageEnd,
                                    Sys = u.Sys,
                                    Dia = u.Dia,
                                    Temp = u.Temp,
                                    Ritm = u.Ritm,

                                    EndSys = u.EndSys,
                                    EndDia = u.EndDia,
                                    EndTemp = u.EndTemp,
                                    EndRitm = u.EndRitm,
                                    Patient = up
                                }
                          ).OrderByDescending(p => p.Id).ToListAsync();

            if (allVal != null)
            {
                foreach (var item in allVal)
                {
                    item.HDSessionHours =  await _dbContext.HDSessionHour.Where(p => p.HDSessionId.Equals(item.Id)).ToListAsync();
                    item.HDSessionPauses =  await _dbContext.HDSessionPause.Where(p => p.HDSessionId.Equals(item.Id)).ToListAsync();
                }
            }

            return await Result<List<AllHDSessionDto>>.SuccessAsync(allVal);
        }

        public async Task<Result<List<AllHDSessionDto>>> AllHDSessionsd(DateTime? fromDate, DateTime? toDate)
        {
            var item1 = await (from u in _dbContext.HDSession where (u.StatusId.Equals((long)HDSessionEnum.Finished)) select u).ToListAsync();

            var allVal = await (from u in _dbContext.HDSession
                                join up in _dbContext.Patient on u.Inn equals up.Inn
                                join c in _dbContext.MedCenter on u.MedCenterId equals c.Id
                                join cm in _dbContext.MedCenterMachine on u.MachineId equals cm.Id
                                join dist in _dbContext.District on c.DistrictId equals dist.Id
                                where (u.StatusId.Equals((long)HDSessionEnum.SendToPay) || u.StatusId.Equals((long)HDSessionEnum.Finished) || u.StatusId.Equals((long)HDSessionEnum.Payed))
                                && (fromDate == null || u.CreatedOn >= fromDate)
                                && (toDate == null || u.CreatedOn <= toDate)
                                select new AllHDSessionDto
                                {
                                    Id = u.Id,
                                    Inn = u.Inn,
                                    Fio = up.LastName + " " + up.FirstName + " " + up.MiddleName!,
                                    MedCenterTitle = c.Title,
                                    District = dist.Title,
                                    Region = "",
                                    Age = (DateTime.Today.Year - up.BirthDate.Value.Year).ToString(),
                                    MachineTitle = cm.Number + " " + cm.Name + " " + cm.Model,
                                    StatusTitle = "",
                                    GenderTitle = (up.Gender == true ? "Муж" : "Жен"),
                                    Condition = u.Condition,
                                    Complaints = u.Complaints,
                                    Program = u.Program,
                                    Dialyzer = u.Dialyzer,
                                    Correction = u.Correction,
                                    Reinfusion = u.Reinfusion,
                                    Access = u.Access,
                                    Anticoagulation = u.Anticoagulation,
                                    Uf = u.Uf,
                                    Speed = u.Speed,
                                    TypeDialyzer = u.TypeDialyzer,
                                    Durators = u.Durators,
                                    StartWeight = u.StartWeight,
                                    EndWeight = u.EndWeight,
                                    Note = u.Note,
                                    SessionStart = u.SessionStart,
                                    SessionEnd = u.SessionEnd,
                                    StatusId = u.StatusId,
                                    TotalMinutes = u.TotalMinutes,
                                    TotalHours = u.TotalHours,
                                    ActivePrice = u.ActivePrice,
                                    SetPrice = u.SetPrice,
                                    Note2 = u.Note2,
                                    Note3 = u.Note3,
                                    ImageStart = u.ImageStart,
                                    ImageEnd = u.ImageEnd,
                                    Sys = u.Sys,
                                    Dia = u.Dia,
                                    Temp = u.Temp,
                                    Ritm = u.Ritm,

                                    EndSys = u.EndSys,
                                    EndDia = u.EndDia,
                                    EndTemp = u.EndTemp,
                                    EndRitm = u.EndRitm,
                                    Patient = up
                                }
                          ).OrderByDescending(p => p.Id).ToListAsync();

            /*if (allVal != null)
            {
                foreach (var item in allVal)
                {
                    item.HDSessionHours =  await _dbContext.HDSessionHour.Where(p => p.HDSessionId.Equals(item.Id)).ToListAsync();
                    item.HDSessionPauses =  await _dbContext.HDSessionPause.Where(p => p.HDSessionId.Equals(item.Id)).ToListAsync();
                }
            }*/

            return await Result<List<AllHDSessionDto>>.SuccessAsync(allVal);
        }


        public async Task<Result<EpicrisisRepDto>> EpicrisisRep(DateTime? fromDate, DateTime? toDate)
        {
            var retData = new EpicrisisRepDto();

            var allEp = await (from u in _dbContext.Epicrisis                                
                                where (fromDate == null || u.CreatedOn >= fromDate)
                                && (toDate == null || u.CreatedOn <= toDate)                               
                                select u
                          ).ToListAsync();

            if (allEp != null)
            {
                try
                {
                    var sp1All = (from p in allEp
                                where String.IsNullOrEmpty(p.SaharKrovi) != false
                                select p).ToList();
                    if (sp1All != null) {
                        if (sp1All.Count() > 0) {
                            retData.sp11 = sp1All.Where(t => Convert.ToDouble(t.SaharKrovi) < 1.4).Count().ToString();
                            retData.sp12 = sp1All.Where(t => Convert.ToDouble(t.SaharKrovi) == 1.4).Count().ToString();
                            retData.sp13 = sp1All.Where(t => Convert.ToDouble(t.SaharKrovi) > 1.4).Count().ToString();
                            retData.sp14 = (Convert.ToInt32(retData.sp11) + Convert.ToInt32(retData.sp12) + Convert.ToInt32(retData.sp13)).ToString();
                        }
                    }

                    var sp2All = (from p in allEp
                                  where String.IsNullOrEmpty(p.Ng) != false
                                  select p).ToList();
                    if (sp2All != null)
                    {
                        if (sp2All.Count() > 0)
                        {
                            retData.sp21 = sp2All.Where(t => Convert.ToDouble(t.Ng) < 95).Count().ToString();
                            retData.sp22 = sp2All.Where(t => Convert.ToDouble(t.Ng) >= 95 && Convert.ToDouble(t.Ng) <= 120).Count().ToString();
                            retData.sp23 = sp2All.Where(t => Convert.ToDouble(t.Ng) > 120).Count().ToString();
                            retData.sp24 = (Convert.ToInt32(retData.sp21) + Convert.ToInt32(retData.sp22) + Convert.ToInt32(retData.sp23)).ToString();
                        }
                    }

                    var sp3All = (from p in allEp
                                  where String.IsNullOrEmpty(p.Fosfor) != false
                                  select p).ToList();
                    if (sp3All != null)
                    {
                        if (sp3All.Count() > 0)
                        {
                            retData.sp31 = sp3All.Where(t => Convert.ToDouble(t.Fosfor) < 1.13).Count().ToString();
                            retData.sp32 = sp3All.Where(t => Convert.ToDouble(t.Fosfor) >= 1.13 && Convert.ToDouble(t.Fosfor) <= 2.0).Count().ToString();
                            retData.sp33 = sp3All.Where(t => Convert.ToDouble(t.Fosfor) > 2.0).Count().ToString();
                            retData.sp34 = (Convert.ToInt32(retData.sp31) + Convert.ToInt32(retData.sp32) + Convert.ToInt32(retData.sp33)).ToString();
                        }
                    }

                    var sp4All = (from p in allEp
                                  where String.IsNullOrEmpty(p.Ht) != false
                                  select p).ToList();
                    if (sp4All != null)
                    {
                        if (sp4All.Count() > 0)
                        {
                            retData.sp41 = sp4All.Where(t =>  Convert.ToDouble(t.Ht) < 2.1).Count().ToString();
                            retData.sp42 = sp4All.Where(t =>  Convert.ToDouble(t.Ht) >= 2.1 && Convert.ToDouble(t.Ht) <= 2.5).Count().ToString();
                            retData.sp43 = sp4All.Where(t =>  Convert.ToDouble(t.Ht) > 2.5).Count().ToString();
                            retData.sp44 = (Convert.ToInt32(retData.sp41) + Convert.ToInt32(retData.sp42) + Convert.ToInt32(retData.sp43)).ToString();
                        }
                    }

                    var sp5All = (from p in allEp
                                  where String.IsNullOrEmpty(p.Eritropoetin) != false
                                  select p).ToList();
                    if (sp5All != null)
                    {
                        if (sp5All.Count() > 0)
                        {

                            retData.sp51 = sp5All.Where(t => Convert.ToDouble(t.Eritropoetin) < 130).Count().ToString();
                            retData.sp52 = sp5All.Where(t => Convert.ToDouble(t.Eritropoetin) >= 130 && Convert.ToDouble(t.Eritropoetin) <= 600).Count().ToString();
                            retData.sp53 = sp5All.Where(t => Convert.ToDouble(t.Eritropoetin) > 600).Count().ToString();
                            retData.sp54 = (Convert.ToInt32(retData.sp51) + Convert.ToInt32(retData.sp52) + Convert.ToInt32(retData.sp53)).ToString();
                        }
                    }
                }
                catch (Exception ex) {
                }

            }


            return await Result<EpicrisisRepDto>.SuccessAsync(retData);
        }

        public async Task<Result<List<PatientSessionsDto>>> GetPatientSessions(string inn)
        {
            var allVal = await (from h in _dbContext.HDSession
                               join up in _dbContext.Patient on h.Inn equals up.Inn
                               join c in _dbContext.MedCenter on h.MedCenterId equals c.Id
                               where (h.Inn.Equals(inn))
                               select new PatientSessionsDto
                               {
                                   Id = h.Id,
                                   Fio = up.LastName + " " + up.FirstName + " " + up.MiddleName!,
                                   MedCenterTitle = c.Title,
                                   SessionStart = h.SessionStart,
                                   SessionEnd = h.SessionEnd,
                                   EffectDate = h.TotalMinutes,
                                   Patient = up
                               }
                         ).OrderByDescending(p => p.Id).ToListAsync();
            return await Result<List<PatientSessionsDto>>.SuccessAsync(allVal);
        }
    }
}


