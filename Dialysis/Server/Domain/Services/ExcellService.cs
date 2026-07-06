using System;
using Dialysis.Server.Domain;
using Dialysis.Shared.Params;
using Dialysis.Shared.Responses;
using Microsoft.AspNetCore.Identity;
using Dialysis.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Dialysis.Shared.Dto;
using Dialysis.Shared.Constants;
using Dialysis.Server.Data;
using DocumentFormat.OpenXml.InkML;
using Dialysis.Client.Pages.References;
using System.Linq.Dynamic.Core;

namespace Dialysis.Server.Domain.Services
{ public interface IExcellService
    {
        Task<Result<AccountRegMedCenterDto>> AccRegistry(long medCenterId, DateTime? fromDate, DateTime? toDate,long? statusId);
        Task<Result<RegOblDto>> AccRegistryObl(long oblId, DateTime? fromDate, DateTime? toDate);
    }

    public class ExcellService : IExcellService
    {

        private readonly AppDbContext _dbContext;
        private readonly IActiveUserService _currentUser;

        public ExcellService(AppDbContext dbContext, IActiveUserService currentUser)
        {
            _dbContext = dbContext;
            _currentUser = currentUser;
        }

        public async Task<Result<AccountRegMedCenterDto>> AccRegistry(long medCenterId, DateTime? fromDate, DateTime? toDate, long? statusId)
        {
            toDate = toDate?.AddHours(23).AddMinutes(59);

            var query = from u in _dbContext.HDSession
                        join up in _dbContext.Patient on u.Inn equals up.Inn
                        where (u.StatusId == (long)HDSessionEnum.SendToPay || u.StatusId == (long)HDSessionEnum.Payed)
                            && (medCenterId == 0 || u.MedCenterId == medCenterId)
                            && (fromDate == null || u.SessionEnd >= fromDate)
                            && (toDate == null || u.SessionEnd <= toDate)
                            && (statusId == 0 || u.StatusId == statusId)
                        let duration = u.TotalHours
                        group new { u, duration } by new 
                        { 
                            Fio = up.LastName + " " + up.FirstName + " " + up.MiddleName,
                            u.Inn,
                            u.MedCenterId,
                            u.ActivePrice
                        } into g
                        select new AccountRegistryDto
                        {
                            Fio = g.Key.Fio,
                            Inn = g.Key.Inn,
                            MedcenterId = (long)g.Key.MedCenterId,
                            CountHd = g.Count(),
                            TotalPrice = (decimal)(g.Count() * g.Key.ActivePrice),
                            OrderNo = 0 // Sonradan doldurulacak
                        };

            var accountRegistries = await query.OrderBy(p => p.Fio).ToListAsync();

            if (!accountRegistries.Any())
            {
                return await Result<AccountRegMedCenterDto>.SuccessAsync(new AccountRegMedCenterDto
                {
                    MedCenterId = medCenterId,
                    MedCenterTitle = "",
                    AccountRegistries = new List<AccountRegistryDto>()
                });
            }

            // Sıra numarası ekle
            for (int i = 0; i < accountRegistries.Count; i++)
            {
                accountRegistries[i].OrderNo = i + 1;
            }

            var result = new AccountRegMedCenterDto
            {
                MedCenterId = medCenterId,
                MedCenterTitle = "",
                ActivePrice = accountRegistries.First().TotalPrice / accountRegistries.First().CountHd,
                TotalPrice = accountRegistries.Sum(x => x.TotalPrice),
                AccountRegistries = accountRegistries
            };

            return await Result<AccountRegMedCenterDto>.SuccessAsync(result);
        }
        public async Task<Result<RegOblDto>> AccRegistryObl(long oblId, DateTime? fromDate, DateTime? toDate)
        {
            toDate = toDate?.AddHours(23).AddMinutes(59);
            var retVal = new RegOblDto();
            var district = await _dbContext.District.Where(p => p.Id.Equals(oblId)).FirstOrDefaultAsync();
            if (district != null)
            {
                var allVal = await (from u in _dbContext.HDSession
                                    join up in _dbContext.Patient on u.PatientId equals up.Id
                                    join um in _dbContext.MedCenter on u.MedCenterId equals um.Id
                                    where (u.StatusId.Equals((long)HDSessionEnum.Payed))
                                    &&(oblId == 0 || um.DistrictId.Equals(oblId))
                                    && (fromDate == null || u.SessionEnd >= fromDate)
                                    && (toDate == null || u.SessionEnd <= toDate)
                                    select new RegMedCenterOblDto
                                    {
                                        Inn=u.Inn,
                                        MedCenterId=um.Id,
                                        MedCenterTitle=um.Title,
                                        SessionCount = 1,
                                        PatientCount =1,
                                        TotalPrice=(decimal)u.ActivePrice,
                                        OblId=um.DistrictId,
                                    }
                          ).OrderBy(p => p.MedCenterId).ToListAsync();
                if (allVal != null)
                {
                    int i = 0;
                    retVal.OblTitle = district.Title;
                    retVal.ActivePrice=allVal.Select(p=>p.TotalPrice).FirstOrDefault();
                    var newList = new List<RegMedCenterOblDto>();
                    foreach (var item in allVal)
                    {
                        if (newList.Any(p => p.MedCenterId.Equals(item.MedCenterId)))
                        {

                        }
                        else
                        {
                            i = i + 1;
                            var newitemm = new RegMedCenterOblDto();
                            var newInn = new RegMedCenterOblDto
                            {
                                OrderNo = i,
                                OblId=item.OblId,
                                MedCenterId=item.MedCenterId,
                                MedCenterTitle=item.MedCenterTitle,
                                SessionCount = allVal.Count(p=>p.MedCenterId.Equals(item.MedCenterId)),
                                PatientCount= allVal.Where(p=>p.MedCenterId.Equals(item.MedCenterId)).Select(n=>n.Inn).Distinct().Count(),
                                TotalPrice = allVal.Where(p => p.MedCenterId.Equals(item.MedCenterId)).Sum(p => p.TotalPrice),
                            };
                            newList.Add(newInn);
                        }
                    }
                    retVal.RegMedCenterOblDtos = newList.OrderBy(p => p.OrderNo).ToList();
                }
            }
            return await Result<RegOblDto>.SuccessAsync(retVal);

        }
    }
}

