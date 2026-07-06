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

namespace Dialysis.Server.Domain.Services
{ public interface IReportService
    {
        Task<Result<List<string>>> AllReports();
    }

    public class ReportService : IReportService
    {

        protected ReportDbContext _dbContext { get; set; }

        public ReportService(ReportDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<List<string>>> AllReports()
        {
            var repList = _dbContext.Reports.Select(t => t.Name).ToList();
            if (repList != null)
            {
                return await Result<List<string>>.SuccessAsync(repList);
            }
            else
            {
                return await Result<List<string>>.SuccessAsync("");
            }
           
        }

    }
}

