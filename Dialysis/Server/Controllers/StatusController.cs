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
    [Route("api/status")]
    [ApiController]
    [Authorize]
    public class StatusController : Controller
    {
        private readonly IStatusService _statusService;
        private readonly IActiveUserService _currentUser;

        public StatusController(IStatusService statusService, IActiveUserService currentUser)
        {
            _statusService = statusService;
            _currentUser = currentUser;
        }

        [HttpPost("addstatus")]
        public async Task<ActionResult> AddStatus([FromBody] Status model)
        {
            var response = await _statusService.AddStatus(model);
            return Ok(response);
        }

        [HttpGet("allstatus")]
        public async Task<ActionResult> AllStatus()
        {            var response = await _statusService.AllStatus();
            return Ok(response);
        }

        [HttpPost("addgroup")]
        public async Task<ActionResult> AddGroup([FromBody] PatientGroup model)
        {            
            var response = await _statusService.AddGroup(model);
            return Ok(response);
        }

        [HttpGet("allgroups")]
        public async Task<ActionResult> AllGroups()
        {
            var response = await _statusService.AllGroups();
            return Ok(response);
        }

        [HttpPost("addprice")]
        public async Task<ActionResult> AddPrice([FromBody] ActivePrice model)
        {
            var response = await _statusService.AddPrice(model);
            return Ok(response);
        }

        [HttpGet("allprices")]
        public async Task<ActionResult> AllPrices()
        {
            var response = await _statusService.AllPrices();
            return Ok(response);
        }

        [HttpPost("addanalysis")]
        public async Task<ActionResult> AddAnalysis([FromBody] Analysis model)
        {
            var response = await _statusService.AddAnalysis(model);
            return Ok(response);
        }
        [HttpPost("addcodemkbs")]
        public async Task<ActionResult> AddCodeMKBS([FromBody] CodeMKB model)
        {
            var response = await _statusService.AddCodeMKBs(model);
            return Ok(response);
        }

        [HttpGet("allanalysis")]
        public async Task<ActionResult> AllAnalysis()
        {
            var response = await _statusService.AllAnalysis();
            return Ok(response);
        }
        [HttpGet("allcodemkbs")]
        public async Task<ActionResult> AllCodes()
        {
            var response = await _statusService.AllCodeMKBs();
            return Ok(response);
        }
        [HttpPost("addreason")]
        public async Task<ActionResult> AddReason([FromBody] GroupReason model)
        {
            var response = await _statusService.AddReason(model);
            return Ok(response);
        }

        [HttpGet("allreason")]
        public async Task<ActionResult> AllReason()
        {
            var response = await _statusService.AllReason();
            return Ok(response);
        }

        [HttpPost("addlpu")]
        public async Task<ActionResult> AddLPU([FromBody] GroupLPU model)
        {
            var response = await _statusService.AddLPU(model);
            return Ok(response);
        }

        [HttpGet("allLPU")]
        public async Task<ActionResult> AllLPU()
        {
            var response = await _statusService.AllLPU();
            return Ok(response);
        }

        [HttpPost("addindref")]
        public async Task<ActionResult> AddIndRef([FromBody] IndicatorReference model)
        {
            var response = await _statusService.AddIndRef(model);
            return Ok(response);
        }

        [HttpGet("allindref")]
        public async Task<ActionResult> AllIndRef()
        {
            var response = await _statusService.AllIndRef();
            return Ok(response);
        }


        [HttpPost("addpersontitle")]
        public async Task<ActionResult> AddPersonTitle([FromBody] GroupPersonTitle model)
        {
            var response = await _statusService.AddPersonTitle(model);
            return Ok(response);
        }

        [HttpGet("allPersonTitle")]
        public async Task<ActionResult> AllPersonTitle()
        {
            var response = await _statusService.AllPersonTitle();
            return Ok(response);
        }
        [HttpGet("alldialyzer")]
        public async Task<ActionResult> AllDialyzer()
        {
            var response = await _statusService.AllDialyzer();
            return Ok(response);
        }
        [HttpPost("adddialyzer")]
        public async Task<ActionResult> AddDialyzer([FromBody] DialyzerType model)
        {
            var response = await _statusService.AddDialyzer(model);
            return Ok(response);
        }

        [HttpGet("allmedicine")]
        public async Task<ActionResult> AllMedicine()
        {
            var response = await _statusService.AllMedicine();
            return Ok(response);
        }
        [HttpPost("addmedicine")]
        public async Task<ActionResult> AddMedicine([FromBody] MedicineType model)
        {
            var response = await _statusService.AddMedicine(model);
            return Ok(response);
        }

        [HttpPost("addakt1")]
        public async Task<ActionResult> AddAkt1([FromBody] QualityExam1 model)
        {
            var response = await _statusService.AddAkt1(model);
            return Ok(response);
        }
        [HttpPost("addakt2")]
        public async Task<ActionResult> AddAkt2([FromBody] Exam2Dto model)
        {
            var response = await _statusService.AddAkt2(model);
            return Ok(response);
        }
        [HttpPost("addakt3")]
        public async Task<ActionResult> AddAkt3([FromBody] Exam3Dto model)
        {
            var response = await _statusService.AddAkt3(model);
            return Ok(response);
        }

        [HttpGet("allakt1")]
        public async Task<ActionResult> AllAkt1()
        {
            var response = await _statusService.AllAkt1();
            return Ok(response);
        }
        [HttpGet("allakt2")]
        public async Task<ActionResult> AllAkt2()
        {
            var response = await _statusService.AllAkt1();
            return Ok(response);
        }
        [HttpGet("allakt3")]
        public async Task<ActionResult> AllAkt3()
        {
            var response = await _statusService.AllAkt3();
            return Ok(response);
        }
    }
}


