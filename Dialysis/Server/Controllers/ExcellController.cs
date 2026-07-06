using ClosedXML.Report;
using Dialysis.Server.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Dialysis.Server.Controllers
{
    [Route("api/excell")]
    [ApiController]
    [Authorize]
    public class ExcellController : Controller
    {
        private readonly IExcellService _excellService;
        private readonly ILoggerManager _logger;
        public ExcellController(IExcellService excellService, ILoggerManager logger)
        {
            _excellService = excellService;
            _logger = logger;
        }


        [HttpGet("accregistries")]
        public async Task<ActionResult> AccRegistries([FromQuery] long medCenterId, [FromQuery] string fromDate, [FromQuery] string toDate,[FromQuery] long? statusId)
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

            if (medCenterId == null)
            {
                medCenterId = 0;
            }
            var response = await _excellService.AccRegistry(medCenterId, dtFrom, dtTo,statusId);
            return Ok(response);
        }
        [HttpGet("accregistries2")]
        public async Task<ActionResult> AccRegistries2([FromQuery] long oblId, [FromQuery] string fromDate, [FromQuery] string toDate)
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
            var response = await _excellService.AccRegistryObl(oblId, dtFrom, dtTo);
            return Ok(response);
        }

        [HttpGet("accregistriesdown")]
        public async Task<ActionResult> AccRegistriesDown([FromQuery] long medCenterId, [FromQuery] string fromDate, [FromQuery] string toDate)
        {
            try
            {
                
                DateTime? dtFrom = null;
                DateTime? dtTo = null;
                var statusId = 0;
                string dfrom = "";
                string tfrom = "";

                if (!String.IsNullOrEmpty(fromDate))
                {
                    var year = fromDate.Substring(0, 4);
                    var month = fromDate.Substring(4, 2);
                    var day = fromDate.Substring(6, 2);
                    dtFrom = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));
                    dfrom = ((DateTime)dtFrom).ToString("dd.MM.yyyy");
                }
                if (!String.IsNullOrEmpty(toDate))
                {
                    var year = toDate.Substring(0, 4);
                    var month = toDate.Substring(4, 2);
                    var day = toDate.Substring(6, 2);
                    dtTo = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));
                    tfrom = ((DateTime)dtTo).ToString("dd.MM.yyyy");
                }


                var response = await _excellService.AccRegistry(medCenterId, dtFrom, dtTo,statusId);

                var template = new XLTemplate("wwwroot/ExcellTemplates/AccTemplate.xlsx");
                var mappedData = response.Data;

                var complexdata = new
                {
                    MedCenterTitle = mappedData.MedCenterTitle,
                    FromDate = dfrom,
                    ToDate = tfrom,
                    ActivePrice = mappedData.ActivePrice,
                    Data = mappedData.AccountRegistries
                };


                template.AddVariable(complexdata);
                template.Generate();
                template.Workbook.Worksheets
                    .Worksheet(template.Workbook.Worksheets.First().Name)
                    .ColumnsUsed()
                    .AdjustToContents();

                //template.SaveAs("report.xlsx");
                //template.SaveAs("wwwroot/ExcellTemplates/"+ medCenterId.ToString() + "_СчетРеестр.xlsx");                //Show report
                //Process.Start(new ProcessStartInfo("СчетРеестр.xlsx") { UseShellExecute = true });


                System.IO.Stream spreadsheetStream = new System.IO.MemoryStream();
                template.SaveAs(spreadsheetStream);
                spreadsheetStream.Position = 0;

                return new FileStreamResult(spreadsheetStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = "СчетРеестр.xlsx" };

                //httpResponse.End();
                //return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf, reportName + ".pdf");
                //Show report
                //Process.Start(new ProcessStartInfo("report.xlsx") { UseShellExecute = true });
            }catch(Exception ex)
            {
                //Console.WriteLine(ex.ToString());
                _logger.LogError(ex.ToString());
                return Ok(ex.ToString());
            }

            //Result<AccountRegMedCenterDto>.SuccessAsync(retVal);
            //return Ok();
        }


        [HttpGet("GetFile")]
        public ActionResult GetFile([FromQuery] long medCenterId)
        {
            return File(System.IO.File.OpenRead("wwwroot/ExcellTemplates/" + medCenterId.ToString() + "_СчетРеестр.xlsx"), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        [HttpGet("GetDocxFile")]
        public ActionResult GetDocxFile([FromQuery] int docId)
        {
            if(docId == 1)
            {
                return File(System.IO.File.OpenRead("wwwroot/Wordfiles/Акт_медицинской_экспертизы_форма11.docx"), "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "Акт_медицинской_экспертизы_форма11.docx");
            }
            else if (docId == 2)
            {
                return File(System.IO.File.OpenRead("wwwroot/Wordfiles/Акт_медицинской_экспертизы_форма11_1.docx"), "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "Акт_медицинской_экспертизы_форма11_1.docx");
            }
            else if (docId == 2)
            {
                return File(System.IO.File.OpenRead("wwwroot/Wordfiles/Акт_медицинской_экспертизы_форма11_2.docx"), "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "Акт_медицинской_экспертизы_форма11_2.docx");
            }
            else
            {
                return File(System.IO.File.OpenRead("wwwroot/Wordfiles/Акт_медицинской_экспертизы_форма11.docx"), "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "Акт_медицинской_экспертизы_форма11.docx");
            }
            
        }

    }
}


