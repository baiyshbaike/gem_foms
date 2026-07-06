using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dialysis.Server.Domain.Services;
using Dialysis.Shared.Dto;
using Dialysis.Shared.Models;
using Dialysis.Shared.Params;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Dialysis.Server.Controllers
{
    [Route("api/report")]
    [ApiController]
    [Authorize]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;
        private readonly IActiveUserService _currentUser;
        public ReportController(IReportService reportService, IActiveUserService currentUser)
        {
            _reportService = reportService;
            _currentUser = currentUser;
        }

        [HttpGet("allreports")]
        public async Task<ActionResult> AllReports()
        {
            var response = await _reportService.AllReports();
            return Ok(response);
        }

       
    }
}


