using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dialysis.Server.Domain.Services;
using Dialysis.Shared.Models;
using Dialysis.Shared.Params;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Dialysis.Server.Controllers
{
    [Route("api/medcenter")]
    [ApiController]
    [Authorize]
    public class MedCenterController : Controller
    {
        private readonly IMedCenterService _medcenterService;
        private readonly IActiveUserService _currentUser;
        private readonly ILoggerManager _logger;
        public MedCenterController(IMedCenterService medcenterService, IActiveUserService currentUser, ILoggerManager logger)
        {
            _medcenterService = medcenterService;
            _currentUser = currentUser;
            _logger = logger;
        }

        [HttpPost("addnew")]
        public async Task<ActionResult> AddNew([FromBody] MedCenter model)
        {
            //if (model != null) model.CreatedBy = _currentUser.UserId;
            var response = await _medcenterService.AddMedcenter(model);
            return Ok(response);
        }

        [HttpGet("allmedcenters")]
        public async Task<ActionResult> AllMedcenters()
        {
            //_logger.LogInfo("Here is info message from the controller.");
            //_logger.LogDebug("Here is debug message from the controller.");
            //_logger.LogWarn("Here is warn message from the controller.");
            //_logger.LogError("Here is error message from the controller.");
            var response = await _medcenterService.AllMedcenter();
            return Ok(response);
        }
        [HttpGet("medcenterschart")]
        public async Task<ActionResult> MedCentersChart()
        {
            //_logger.LogInfo("Here is info message from the controller.");
            //_logger.LogDebug("Here is debug message from the controller.");
            //_logger.LogWarn("Here is warn message from the controller.");
            //_logger.LogError("Here is error message from the controller.");
            var response = await _medcenterService.MedCentersChart();
            return Ok(response);
        }

        [HttpPost("addmachine")]
        public async Task<ActionResult> AddMachine([FromBody] MachineAddParams model)
        {
            var response = await _medcenterService.AddMachine(model);
            return Ok(response);
        }

        [HttpGet("allmachines")]
        public async Task<ActionResult> AllMachines([FromQuery] long Id)
        {
            var response = await _medcenterService.MedCenterMachines(Id);
            return Ok(response);
        }
        [HttpGet("allempoyees")]
        public async Task<ActionResult> AllEmployees([FromQuery] long Id)
        {
            var response = await _medcenterService.AllEmployees(Id);
            return Ok(response);
        }
        [HttpPost("addeditemployee")]
        public async Task<ActionResult> AddEditEmployee([FromBody] MedCenterEmployee model)
        {
            var response = await _medcenterService.AddEditEmployee(model);
            return Ok(response);
        }
        [HttpPost("deleteemployee")]
        public async Task<ActionResult> DeleteEmployee([FromBody] MedCenterEmployee model)
        {
            var response = await _medcenterService.DeleteEmployee(model);
            return Ok(response);
        }
        [HttpGet("downloadfile/{fileName}")]
        public IActionResult DownloadFile(string fileName)
        {
            var filePath = Path.Combine("wwwroot", "UploadFiles", "Employees", fileName);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/octet-stream", fileName);
        }
        [HttpGet("allmachinesdto")]
        public async Task<ActionResult> AllMachinesDto()
        {
            var response = await _medcenterService.AllMachinesDto();
            return Ok(response);
        }

        [HttpGet("relmedcenters")]
        public async Task<ActionResult> RelMedcenters()
        {
            var userId = _currentUser.UserId;
            if (userId != null || userId!=0)
            {
                var response = await _medcenterService.UserMedcenter(userId);
                return Ok(response);
            }
            else
            {              
                return Ok("Empty data");
            }
           
        }

       

        [HttpGet("usercenter")]
        public async Task<ActionResult> UserCenter([FromQuery] long Id)
        {
            var response = await _medcenterService.UserMedcenter(Id);
            return Ok(response);
        }

        [HttpGet("addremoveuser")]
        public async Task<ActionResult> AddRemUser([FromQuery] long Id, [FromQuery] long UserId,  [FromQuery] int status)
        {
            var response = await _medcenterService.AddRemoveUser(Id, UserId,status);
            return Ok(response);
        }

        [HttpPost("addindicator")]
        public async Task<ActionResult> AddIndicator([FromBody] IndicatorPostArgs model)
        {
            var response = await _medcenterService.AddIndicator(model);
            return Ok(response);
        }

        [HttpGet("allindicator")]
        public async Task<ActionResult> AllIndicator([FromQuery] long Id)
        {
            var response = await _medcenterService.AllIndicator(Id);
            return Ok(response);
        }

        [HttpGet("inddetail")]
        public async Task<ActionResult> IndDetail([FromQuery] long Id)
        {
            var response = await _medcenterService.IndDetail(Id);
            return Ok(response);
        }

        [HttpGet("medcentersbydistrict")]
        public async Task<ActionResult> MedCentersByDistrict()
        {
            var response = await _medcenterService.MedCentersByDistrict();
            return Ok(response);
        }

        [HttpGet("patientagegroup")]
        public async Task<ActionResult> PatientAgeGroup()
        {
            var response = await _medcenterService.PatientByAgeGroup();
            return Ok(response);
        }
        [HttpPost("deletemashinebyid")]
        public async Task<ActionResult> DeleteMashineById([FromBody] long id)
        {
            var response = await _medcenterService.DeleteMashineById(id);
            return Ok(response);
        }
    }
}


