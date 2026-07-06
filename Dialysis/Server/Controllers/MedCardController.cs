using Dialysis.Server.Domain.Services;
using Dialysis.Shared.Dto;
using Dialysis.Shared.Models;
using Dialysis.Shared.Params;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Dialysis.Server.Controllers
{
    [Route("api/medcard")]
    [ApiController]
    [Authorize]
    public class MedCardController : Controller
    {
        private readonly IMedCardService _medcardService;
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
        public MedCardController(IMedCardService medcardService, IActiveUserService currentUser)
        {
            _medcardService = medcardService;
            _currentUser = currentUser;
        }

        [HttpPost("importexcelfile")]
        public async Task<ActionResult> ImportExcelFile([FromBody] List<ImportHDSessionsDto> models)
        {
            var response = await _medcardService.ImportExcelFile(models);
            return Ok(response);
        }
        [HttpPost("addcard")]
        public async Task<ActionResult> AddCard([FromBody] MedCard model)
        {
            //if (model != null) model.CreatedBy = _currentUser.UserId;
            var response = await _medcardService.AddCard(model);
            return Ok(response);
        }

        [HttpPost("addcardall")]
        public async Task<ActionResult> AddCardAll([FromBody] MedCardArgs model)
        {
            //if (model != null) model.CreatedBy = _currentUser.UserId;
            try
            {
                var response = await _medcardService.AddCardAll(model);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return BadRequest();
        }

        [HttpGet("searchcard")]
        public async Task<ActionResult> SearchCard([FromQuery] string? Inn, [FromQuery]  string? LastName, [FromQuery]  string? Name)
        {
            var response = await _medcardService.SearchCard(Inn, LastName, Name);
            return Ok(response);
        }

        [HttpGet("searchcard2")]
        public async Task<ActionResult> SearchCardByMCenter([FromQuery] string? Inn, [FromQuery] long? Mid)
        {
            if (Mid == null) Mid = 0;
            var response = await _medcardService.SearchCardByMCenter(Inn, (long)Mid);
            return Ok(response);
        }

        [HttpGet("allmedcards")]
        public async Task<ActionResult> AllMedCards()
        {
            var response = await _medcardService.AllMedCards();
            return Ok(response);
        }

        [HttpGet("carddetail")]
        public async Task<ActionResult> CardDetail([FromQuery] long medcardId)
        {
            var response = await _medcardService.MedCardDetail(medcardId);
            return Ok(response);
        }
        
        [HttpGet("isaccesscard")]
        public async Task<ActionResult> IsAccessCard([FromQuery] long medcardId)
        {
            var response = await _medcardService.IsAccessCard(medcardId);
            return Ok(response);
        }

        [HttpGet("isowncard")]
        public async Task<ActionResult> IsOwnCard([FromQuery] long medcardId)
        {
            var response = await _medcardService.IsOwnCard(medcardId);
            return Ok(response);
        }
        [HttpGet("gethdsessionbyid")]
        public async Task<ActionResult> GetSessionById([FromQuery] long Id)
        {
            var response = await _medcardService.GetSessionById(Id);
            return Ok(response);
        }

        [HttpGet("allidentify")]
        public async Task<ActionResult> AllIdentify([FromQuery] long medCenterId)
        {
            var response = await _medcardService.AllIdentify(medCenterId);
            return Ok(response);
        }
        [HttpGet("allidentifyasync")]
        public async Task<ActionResult> AllIdentifyAsync()
        {
            var response = await _medcardService.AllIdentifyAsync();
            return Ok(response);
        }
        [HttpPost("addidentify")]
        public async Task<ActionResult> AddIdentify([FromBody] HDSession model)
        {
            var response = await _medcardService.AddIdentify(model);
            return Ok(response);
        }
        [HttpPost("endidentify")]
        public async Task<ActionResult> EndIdentify([FromBody] HDSession model)
        {
            var response = await _medcardService.EndIdentify(model);
            return Ok(response);
        }

        [HttpGet("getidentify")]
        public async Task<ActionResult> GetIdentify([FromQuery] long mid, [FromQuery] string inn)
        {
            var response = await _medcardService.GetIdentify(inn, mid);
            return Ok(response);
        }

        [HttpPost("sendtopay")]
        public async Task<ActionResult> SendToPay([FromBody] HDSession model)
        {
            var response = await _medcardService.SendToPay(model);
            return Ok(response);
        }
        [HttpPost("deletehdsession")]
        public async Task<ActionResult> DeleteHDSession([FromBody] HDSession model)
        {
            var response = await _medcardService.DeleteHDSession(model);
            return Ok(response);
        }

        [HttpPost("archivehdsessionbyid")]
        public async Task<ActionResult> ArchiveHdSessionById([FromBody] long id)
        {
            var response = await _medcardService.ArchiveHdSessionById(id);
            return Ok(response);
        }
        [HttpPost("deletehdsessionbyid")]
        public async Task<ActionResult> DeleteHDSessionById([FromBody] long id)
        {
            var response = await _medcardService.DeleteHDSessionById(id);
            return Ok(response);
        }
        [HttpPost("checktopay")]
        public async Task<ActionResult> CheckToPay([FromBody] HDSession model)
        {
            var response = await _medcardService.ChekToPay(model);
            return Ok(response);
        }
        [HttpPost("checktopayall")]
        public async Task<ActionResult> CheckToPayAll([FromBody] List<HDSession> model)
        {
            var response = await _medcardService.ChekToPayAll(model);
            return Ok(response);
        }
        [HttpPost("sendtoend")]
        public async Task<ActionResult> SendToEnd([FromBody] HDSession model)
        {
            var response = await _medcardService.SendToEnd(model);
            return Ok(response);
        }


        [HttpPost("startsession")]
        public async Task<ActionResult> StartSession([FromBody] HDSession model)
        {
            var response = await _medcardService.StartSession(model);
            return Ok(response);
        }
        [HttpPost("addsession")]
        public async Task<ActionResult> AddSession([FromBody] HDSession model)
        {
            var response = await _medcardService.AddSession(model);
            return Ok(response);
        }
        [HttpPost("editsession")]
        public async Task<ActionResult> EditSession([FromBody] HDSession model)
        {
            var response = await _medcardService.EditSession(model);
            return Ok(response);
        }
        [HttpPost("addsessionstart")]
        public async Task<ActionResult> AddSessionStart([FromBody] HDSession model)
        {
            var response = await _medcardService.AddSessionStart(model);
            return Ok(response);
        }

        [HttpGet("bymedcard")]
        public async Task<ActionResult> bymedcard([FromQuery] long medcardId, [FromQuery] int? iscard)
        {
            if (iscard != null)
            {
                if (iscard > 0)
                {
                    var response = await _medcardService.ByMedCardId(medcardId,3);
                    return Ok(response);
                }
                else
                {
                    var response = await _medcardService.ByMedCardId(medcardId,0);
                    return Ok(response);
                }
            }
            else
            {
                var response = await _medcardService.ByMedCardId(medcardId,0);
                return Ok(response);
            }
        }


        [HttpGet("activesessions")]
        public async Task<ActionResult> activesessions([FromQuery] long medCenterId, [FromQuery] string isonly)
        {
            var response = await _medcardService.ActiveSessions(medCenterId, isonly);
            return Ok(response);
        }

        [HttpPost("addindicator")]
        public async Task<ActionResult> AddIndicator([FromBody] HDSessionHour model)
        {
            var response = await _medcardService.AddIndicator(model);
            return Ok(response);
        }

        [HttpPost("updateindicator")]
        public async Task<ActionResult> UpdateIndicator([FromBody] HDSessionHour model)
        {
            var response = await _medcardService.UpdateIndicator(model);
            return Ok(response);
        }

        [HttpGet("getindicator")]
        public async Task<ActionResult> GetIndicator([FromQuery] long sessionId, [FromQuery] string hour)
        {
            var response = await _medcardService.GetIndicator(sessionId, hour);
            return Ok(response);
        }


        [HttpPost("addpause")]
        public async Task<ActionResult> AddPause([FromBody] HDSessionPause model)
        {
            var response = await _medcardService.AddPause(model);
            return Ok(response);
        }

        [HttpGet("unpause")]
        public async Task<ActionResult> UnPause([FromQuery] long sessionId)
        {
            var response = await _medcardService.ContinuePause(sessionId, 99);
            return Ok(response);
        }


        [HttpPost("endsession")]
        public async Task<ActionResult> EndSession([FromBody] HDSession model)
        {
            var response = await _medcardService.EndSession(model);
            return Ok(response);
        }

        [HttpGet("allsessions")]
        public async Task<ActionResult> Allsessions([FromQuery] long medCenterId, [FromQuery] string fromDate, [FromQuery] string toDate, [FromQuery] long? statusId, [FromQuery] long? hour)
        {
            DateTime? dtFrom = ParseDate(fromDate);
            DateTime? dtTo = ParseDate(toDate);

            var response = await _medcardService.AllSessions(medCenterId, dtFrom, dtTo, statusId, (long)hour);
            return Ok(response);
        }
        [HttpGet("getpatientinfo")]
        public async Task<ActionResult> GetPatientInfo([FromQuery] long medCenterId, [FromQuery] string fromDate, [FromQuery] string toDate)
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
                dtTo = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day)).AddDays(1);
            }
            var response = await _medcardService.GetPatientInfo(medCenterId, dtFrom, dtTo);
            return Ok(response);
        }

        [HttpGet("finishedsessions")]
        public async Task<ActionResult> FinishedSessions([FromQuery] long medCenterId, [FromQuery] string isonly)
        {
            var response = await _medcardService.FinishedSessions(medCenterId, isonly);
            return Ok(response);
        }

        [HttpGet("sessiondetail")]
        public async Task<ActionResult> SessionDetail([FromQuery] long sessionId)
        {
            var response = await _medcardService.SessionDetail(sessionId);
            return Ok(response);
        }

        #region medcard extra
        [HttpPost("addinspection")]
        public async Task<ActionResult> AddInspection([FromBody] FirstInspection model)
        {
            try
            {
                var response = await _medcardService.AddInspection(model);
                return Ok(response);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return BadRequest();
        }
        [HttpGet("getinspection")]
        public async Task<ActionResult> GetInspection([FromQuery] long medcardId)
        {
            var response = await _medcardService.GetInspection(medcardId);
            return Ok(response);
        }

        [HttpPost("addrespiratory")]
        public async Task<ActionResult> AddRespiratory([FromBody] FirstRespiratory model)
        {
            try
            {
                var response = await _medcardService.AddRespiratoty(model);
                return Ok(response);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return BadRequest();
        }
        [HttpGet("getrespiratory")]
        public async Task<ActionResult> GetRespiratory([FromQuery] long medcardId)
        {
            var response = await _medcardService.GetRespiratoty(medcardId);
            return Ok(response);
        }

        [HttpPost("addfan")]
        public async Task<ActionResult> AddFan([FromBody] FirstAnalysis model)
        {
            var response = await _medcardService.AddFirstAn(model);
            return Ok(response);
        }
        [HttpGet("getfan")]
        public async Task<ActionResult> GetFan([FromQuery] long medcardId)
        {
            var response = await _medcardService.GetFirstAn(medcardId);
            return Ok(response);
        }


        [HttpPost("addcardio")]
        public async Task<ActionResult> AddCardio([FromBody] FirstCardiovascular model)
        {
            var response = await _medcardService.AddCardiovascular(model);
            return Ok(response);
        }
        [HttpGet("getcardio")]
        public async Task<ActionResult> GetCardio([FromQuery] long medcardId)
        {
            var response = await _medcardService.GetCardiovascular(medcardId);
            return Ok(response);
        }


        [HttpPost("addconfec")]
        public async Task<ActionResult> AddConfec([FromBody] FirstConfectionery model)
        {
            var response = await _medcardService.AddConfectionery(model);
            return Ok(response);
        }
        [HttpGet("getconfec")]
        public async Task<ActionResult> GetConfec([FromQuery] long medcardId)
        {
            var response = await _medcardService.GetConfectionery(medcardId);
            return Ok(response);
        }


        [HttpPost("addendocrine")]
        public async Task<ActionResult> AddEndocrine([FromBody] FirstEndocrine model)
        {
            var response = await _medcardService.AddEndocrine(model);
            return Ok(response);
        }
        [HttpGet("getendocrine")]
        public async Task<ActionResult> GetEndocrine([FromQuery] long medcardId)
        {
            var response = await _medcardService.GetEndocrine(medcardId);
            return Ok(response);
        }


        [HttpPost("addneuro")]
        public async Task<ActionResult> AddNeuro([FromBody] FirstNeuro model)
        {
            var response = await _medcardService.AddNeuro(model);
            return Ok(response);
        }
        [HttpGet("getneuro")]
        public async Task<ActionResult> GetNeuro([FromQuery] long medcardId)
        {
            var response = await _medcardService.GetNeuro(medcardId);
            return Ok(response);
        }

        [HttpPost("addurogen")]
        public async Task<ActionResult> AddUrogen([FromBody] FirstUrogenital model)
        {
            var response = await _medcardService.AddUrogenital(model);
            return Ok(response);
        }
        [HttpGet("geturogen")]
        public async Task<ActionResult> GetUrogen([FromQuery] long medcardId)
        {
            var response = await _medcardService.GetUrogenital(medcardId);
            return Ok(response);
        }
        
        [HttpPost("addepicrisis")]
        public async Task<ActionResult> AddEpicrisis([FromBody] Epicrisis model)
        {
            var response = await _medcardService.AddEpicrisis(model);
            return Ok(response);
        }
        [HttpGet("getepicrisis")]
        public async Task<ActionResult> GetEpicrisis([FromQuery] long medcardId)
        {
            var response = await _medcardService.GetEpicrisis(medcardId);
            return Ok(response);
        }
        [HttpGet("getepicrisisdetail")]
        public async Task<ActionResult> GetEpicrisisDetail([FromQuery] long Id)
        {
            var response = await _medcardService.GetEpicrisisDetail(Id);
            return Ok(response);
        }
        [HttpGet("getepicrisislist")]
        public async Task<ActionResult> GetEpicrisisList([FromQuery] long medcardId)
        {
            var response = await _medcardService.GetEpicrisisList(medcardId);
            return Ok(response);
        }

        #endregion


        [HttpPost("addanalyses")]
        public async Task<ActionResult> AddAnalyses([FromBody] AnalysisAddArgs model)
        {
            var response = await _medcardService.AddAnalyses(model);
            return Ok(response);
        }

        [HttpGet("analysesbymedcenter")]
        public async Task<ActionResult> AnalysesByMedCenter([FromQuery] long medCenterId, [FromQuery] string fromDate, [FromQuery] string toDate,[FromQuery] decimal? fAn, [FromQuery] decimal? tAn)
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
                dtTo = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day)).AddDays(1);
            }
            decimal fAnLocal = 0;
            if (fAn != null) fAnLocal = (decimal)fAn;
            decimal  tAnLocal = 0;
            if (tAn != null) tAnLocal = (decimal)tAn;
            var response = await _medcardService.AnalysesByMedCenter(medCenterId,dtFrom, dtTo,fAnLocal, tAnLocal);
            return Ok(response);
        }
        [HttpGet("analysesbyinn")]
        public async Task<ActionResult> AnalysesByInn([FromQuery] string inn, [FromQuery] string? fromDate, [FromQuery] string? toDate)
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
                dtTo = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day)).AddDays(1);
            }
            var response = await _medcardService.AnalysesByInn(inn, dtFrom, dtTo);
            return Ok(response);
        }

        [HttpGet("analysesfilter")]
        public async Task<ActionResult> AnalysesFilter([FromQuery] long? medCenterId,  [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] decimal? fAn, [FromQuery] decimal? tAn,[FromQuery] long? aId)
        {
            long aIdLocal = 0;
            if (aId != null) aIdLocal = (long)aId;
            decimal fAnLocal = 0;
            if (fAn != null) fAnLocal = (decimal)fAn;
            decimal  tAnLocal = 0;
            if (tAn != null) tAnLocal = (decimal)tAn;
            if (medCenterId == null) medCenterId = 0;

            var response = await _medcardService.AnalysesByIdAndInn((long)medCenterId, aIdLocal, fromDate, toDate, fAnLocal, tAnLocal);
            return Ok(response);
        }

        [HttpGet("allhdsessions")]
        public async Task<ActionResult> AllHdSessions()
        {
            var response = await _medcardService.AllHDSessions();
            return Ok(response);
        }

        [HttpGet("allhdsessionsd")]
        public async Task<ActionResult> AllHdSessionsd([FromQuery] string fromDate, [FromQuery] string toDate)
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
                dtTo = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day)).AddDays(1);
            }
            var response = await _medcardService.AllHDSessionsd(dtFrom, dtTo);
            return Ok(response);
        }
        [HttpGet("epicrisisrep")]
        public async Task<ActionResult> EpicrisisRep( [FromQuery] string fromDate, [FromQuery] string toDate)
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
                dtTo = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day)).AddDays(1);
            }
            var response = await _medcardService.EpicrisisRep(dtFrom, dtTo);
            return Ok(response);
        }

        [HttpGet("getpatinentsessions")]
        public async Task<ActionResult> GetPaitientSessions([FromQuery] string inn)
        {
            var response = await _medcardService.GetPatientSessions(inn);
            return Ok(response);
        }




    }
}


