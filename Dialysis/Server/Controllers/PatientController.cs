using Dialysis.Server.Domain.Services;
using Dialysis.Shared.Models;
using Dialysis.Shared.Models.Files;
using Dialysis.Shared.Params;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Dialysis.Shared.Dto;


namespace Dialysis.Server.Controllers
{
    [Route("api/patient")]
    [ApiController]
    [Authorize]
    public class PatientController : Controller
    {
        private readonly IPatientService _patientService;
        private readonly IActiveUserService _currentUser;
        private static DateTime? ParseDate(string date)
        {
            if (string.IsNullOrWhiteSpace(date) || date.Length != 8)
                return null;

            if (DateTime.TryParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
            {
                return parsedDate;
            }

            return null;
        }
        public PatientController(IPatientService patientService, IActiveUserService currentUser)
        {
            _patientService = patientService;
            _currentUser = currentUser;
        }
        [HttpGet("med-center-patients")]
        public async Task<ActionResult<PagedResponseDto<PatientInfoDto>>> GetPagedPatients(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int statusId = 0,
            [FromQuery] long medCenterId = 0,
            [FromQuery] string sortBy = "",
            [FromQuery] bool sortAsc = false,
            [FromQuery] string searchString = "",
            [FromQuery] DateTime? createdOnDateFrom = null,
            [FromQuery] DateTime? createdOnDateTo = null,
            [FromQuery] DateTime? birthDateFrom = null,
            [FromQuery] DateTime? birthDateTo = null
            ) 
        {
            var response = await _patientService.GetPatientsPagedAsync(
                createdOnDateFrom,
                createdOnDateTo,
                birthDateFrom,
                birthDateTo,
                pageNumber,
                pageSize,
                statusId,
                medCenterId,
                sortBy,
                sortAsc,
                searchString);
            return Ok(response);
        }
        [HttpGet("patient-files/{patientId}")]
        public async Task<ActionResult> GetPatientFiles(long patientId)
        {
            var files = await _patientService.GetPatientFiles(patientId);
            return Ok(files);
        }

        [HttpGet("download-file/{fileId}")]
        public async Task<ActionResult> DownloadFile(long fileId)
        {
            var file = await _patientService.GetPatientFile(fileId);
            if (file == null) return NotFound();

            var filePath = Path.Combine("wwwroot", file.FilePath);
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var base64String = Convert.ToBase64String(fileBytes);
            return Content(base64String);
        }
        [HttpGet("download-file2/{fileId}")]
        public async Task<ActionResult> DownloadFile2(long fileId)
        {
            var file = await _patientService.GetPatientFile(fileId);
            if (file == null) return NotFound();

            var filePath = Path.Combine("wwwroot", file.FilePath);
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, GetContentType(file.FileName), file.OriginalFileName);
        }

        [HttpDelete("delete-file/{fileId}")]
        public async Task<ActionResult> DeleteFile(long fileId)
        {
            var result = await _patientService.DeletePatientFile(fileId);
            return Ok(result);
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLower();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };
        }
        [HttpPost("med-center-add-edit-patient")]
        public async Task<ActionResult> AddEditMedCenterPatient([FromBody] PatientAddEditDto model)
        {
            var response = await _patientService.AddEditMedCenterPatient(model);
            return Ok(response);
        }
        [HttpPost("med-center-delete-patient")]
        public async Task<ActionResult> DeleteMedCenterPatient([FromBody] Patient model)
        {
            var response = await _patientService.DeleteMedCenterPatient(model);
            return Ok(response);
        }
        [HttpPost("addpatient")]
        public async Task<ActionResult> AddPatient([FromBody] Patient model)
        {
            var response = await _patientService.AddPatient(model);
            return Ok(response);
        }
        [HttpPost("deletepatient")]
        public async Task<ActionResult> DeletePatient([FromBody] Patient model)
        {
            var response = await _patientService.DeletePatient(model);
            return Ok(response);
        }

        [HttpGet("allpatients")]
        public async Task<ActionResult> AllPatients()
        {

            var response = await _patientService.AllPatients();
            return Ok(response);
        }

        [HttpGet("allpatientswithreason")]
        public async Task<ActionResult> AllPatientsWithReason()
        {
            var response = await _patientService.AllPatientsWithReason();
            return Ok(response);
        }
        [HttpPost("addfile")]
        public async Task<IActionResult> SaveFileToServer([FromBody] IList<SaveFile> files)
        {
            var response = await _patientService.AddFile(files);
            return Ok(response);
        }
        [HttpGet("getfiles")]
        public async Task<ActionResult> GetFiles([FromQuery] long id)
        {
            var response = await _patientService.GetFiles(id);
            return Ok(response);
        }
        [HttpPost("deletefile")]
        public async Task<ActionResult> DeleteFile([FromBody] SaveFile model)
        {
            var response = await _patientService.DeleteFile(model);
            return Ok(response);
        }
        [HttpPost("addprotocolfile")]
        public async Task<IActionResult> AddProtocolFile([FromBody] IList<ProtocolFile> files)
        {
            var response = await _patientService.AddProtocolFile(files);
            return Ok(response);
        }
        [HttpGet("getprotocolfiles")]
        public async Task<ActionResult> GetProtocolFiles([FromQuery] long id)
        {
            var response = await _patientService.GetProtocolFiles(id);
            return Ok(response);
        }
        [HttpGet("getprotocolfilesbyinn")]
        public async Task<ActionResult> GetProtocolFilesByInn([FromQuery] string inn)
        {
            var response = await _patientService.GetProtocolFilesByInn(inn);
            return Ok(response);
        }
        [HttpPost("deleteprotocolfile")]
        public async Task<ActionResult> DeleteProtocolFile([FromBody] ProtocolFile model)
        {
            var response = await _patientService.DeleteProtocolFile(model);
            return Ok(response);
        }
        [HttpGet("allpatients2")]
        public async Task<ActionResult> AllPatients2([FromQuery] string fromDate, [FromQuery] string toDate)
        {
            DateTime? dtFrom = null;
            DateTime? dtTo = null;

            if (!String.IsNullOrEmpty(fromDate))
            {
                var year = fromDate.Substring(0, 4);
                var month = fromDate.Substring(4, 2);
                var day = fromDate.Substring(6, 2);
                dtFrom = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));
            }
            if (!String.IsNullOrEmpty(toDate))
            {
                var year = toDate.Substring(0, 4);
                var month = toDate.Substring(4, 2);
                var day = toDate.Substring(6, 2);
                dtTo = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));
            }
            var response = await _patientService.AllPatients2(dtFrom, dtTo);
            return Ok(response);
        }
        [HttpGet("allpatientswithreason2")]
        public async Task<ActionResult> AllPatientsWithReason2([FromQuery] string? fromDate = null, [FromQuery] string? toDate = null, [FromQuery] string? deletedFrom = null, [FromQuery] string? deletedTo = null)
        {
            DateTime? dtFrom = ParseDate(fromDate);
            DateTime? dtTo = ParseDate(toDate);
            DateTime? dDelFrom = ParseDate(deletedFrom);
            DateTime? dDelTo = ParseDate(deletedTo);

            var response = await _patientService.AllPatientsWithReason2(dtFrom, dtTo, dDelFrom, dDelTo);
            return Ok(response);
        }


        [HttpGet("allpatients3")]
        public async Task<ActionResult> AllPatients3([FromQuery] string fromDate, [FromQuery] string toDate)
        {
            DateTime? dtFrom = null;
            DateTime? dtTo = null;

            if (!String.IsNullOrEmpty(fromDate))
            {
                var year = fromDate.Substring(0, 4);
                var month = fromDate.Substring(4, 2);
                var day = fromDate.Substring(6, 2);
                dtFrom = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));
            }
            if (!String.IsNullOrEmpty(toDate))
            {
                var year = toDate.Substring(0, 4);
                var month = toDate.Substring(4, 2);
                var day = toDate.Substring(6, 2);
                dtTo = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));
            }
          
            var response = await _patientService.AllPatients3(dtFrom, dtTo);
            return Ok(response);
        }


        [HttpGet("allpatients4")]
        public async Task<ActionResult> AllPatients4([FromQuery] long? medCenterId, [FromQuery] int totalDays, [FromQuery] string fromDate, [FromQuery] string toDate, [FromQuery] string typePage)
        {
            DateTime? dtFrom = ParseDate(fromDate);
            DateTime? dtTo = ParseDate(toDate);
            if (medCenterId == null) medCenterId = 0;
            var response = await _patientService.AllPatients4((long)medCenterId, totalDays, dtFrom, dtTo, typePage);
            return Ok(response);
        }


        [HttpGet("allpatients5")]
        public async Task<ActionResult> AllPatients5([FromQuery] long? medCenterId, [FromQuery] string fromDate, [FromQuery] string toDate)
        {
            DateTime? dtFrom = null;
            DateTime? dtTo = null;

            if (!String.IsNullOrEmpty(fromDate))
            {
                var year = fromDate.Substring(0, 4);
                var month = fromDate.Substring(4, 2);
                var day = fromDate.Substring(6, 2);
                dtFrom = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));
            }
            if (!String.IsNullOrEmpty(toDate))
            {
                var year = toDate.Substring(0, 4);
                var month = toDate.Substring(4, 2);
                var day = toDate.Substring(6, 2);
                dtTo = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));
            }
            if (medCenterId == null) medCenterId = 0;
            var response = await _patientService.AllPatients5((long)medCenterId, dtFrom, dtTo);
            return Ok(response);
        }

        [HttpGet("searchpatient")]
        public async Task<ActionResult> SearchPatient([FromQuery] string inn)
        {
            var response = await _patientService.SearchByInn(inn);
            return Ok(response);
        }

        [HttpGet("searchresult")]
        public async Task<ActionResult> SearchResult([FromQuery] string inn)
        {
            var response = await _patientService.SearchResultByInn(inn);
            return Ok(response);
        }

        [HttpGet("getbymedcard")]
        public async Task<ActionResult> GetByMedCardId([FromQuery] long medcardId)
        {
            var response = await _patientService.GetByMedCardId(medcardId);
            return Ok(response);
        }

        [HttpGet("getbyid")]
        public async Task<ActionResult> GetById([FromQuery] long id)
        {
            var response = await _patientService.GetById(id);
            return Ok(response);
        }

        [HttpGet("allpatientsbox")]
        public async Task<ActionResult> AllPatientsBox()
        {

            var response = await _patientService.AllPatientsBox();
            return Ok(response);
        }

        [HttpPost("addprotocol")]
        public async Task<ActionResult> AddProtocol([FromBody] GroupProtocolArgs model)
        {
            var response = await _patientService.AddGroupAct(model);
            return Ok(response);
        }

        [HttpPost("allprotocol")]
        public async Task<ActionResult> AllProtocol([FromBody] DateRangeParams dateRange)
        {
            var response = await _patientService.AllGroupActs(dateRange.FromDate, dateRange.ToDate);
            return Ok(response);
        }

        [HttpGet("detailprotocol")]
        public async Task<ActionResult> DetailProtocol([FromQuery] long id)
        {

            var response = await _patientService.GroupActsDetail(id);
            return Ok(response);
        }


        [HttpGet("allchanges")]
        public async Task<ActionResult> AllChanges()
        {

            var response = await _patientService.AllReestrChanges();
            return Ok(response);
        }

        [HttpGet("detailchanges")]
        public async Task<ActionResult> DetailChanges([FromQuery] long id)
        {
            var response = await _patientService.ReestrChangeDetail(id);
            return Ok(response);
        }

        [HttpPost("addwriting")]
        public async Task<ActionResult> AddWriting([FromBody] WritingOut model)
        {
            var response = await _patientService.AddWriting(model);
            return Ok(response);
        }

        [HttpGet("searchwriting")]
        public async Task<ActionResult> SearchWriting([FromQuery] string? inn, [FromQuery] long? medCenterId, [FromQuery] string? fromDate, [FromQuery] string? toDate)
        {
            DateTime? dtFrom = null;
            DateTime? dtTo = null;

            if (!String.IsNullOrEmpty(fromDate))
            {
                var year = fromDate.Substring(0, 4);
                var month = fromDate.Substring(4, 2);
                var day = fromDate.Substring(6, 2);
                dtFrom = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));
            }
            if (!String.IsNullOrEmpty(toDate))
            {
                var year = toDate.Substring(0, 4);
                var month = toDate.Substring(4, 2);
                var day = toDate.Substring(6, 2);
                dtTo = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));
            }
            if (medCenterId == null) medCenterId = 0;
            var response = await _patientService.SearchWritings(inn, (long)medCenterId, dtFrom, dtTo);
            return Ok(response);
        }

        [HttpGet("writingdetail")]
        public async Task<ActionResult> WritingDetail([FromQuery] long id)
        {
            var response = await _patientService.GetWritingById(id);
            return Ok(response);
        }
        [HttpPost("addcomplaint")]
        public async Task<ActionResult> AddComplaint([FromBody] Complaint model)
        {
            var response = await _patientService.AddComplaint(model);
            return Ok(response);
        }
        [HttpGet("searchcomplaint")]
        public async Task<ActionResult> SearchComplaint([FromQuery] long? medCenterId, [FromQuery] string? inn)
        {
            if (medCenterId == null) medCenterId = 0;
            var response = await _patientService.SearchComplaint(inn, (long)medCenterId);
            return Ok(response);
        }

        [HttpGet("complaintdetail")]
        public async Task<ActionResult> ComplaintDetail([FromQuery] long id)
        {
            var response = await _patientService.GetComplaintById(id);
            return Ok(response);
        }
        [HttpGet("getbyinn")]
        public async Task<ActionResult> GetByInn([FromQuery] string inn)
        {
            var response = await _patientService.GetByInn(inn);
            return Ok(response);
        }

    }
}


