using DevExpress.AspNetCore.Reporting.QueryBuilder;
using DevExpress.AspNetCore.Reporting.QueryBuilder.Native.Services;
using DevExpress.AspNetCore.Reporting.ReportDesigner;
using DevExpress.AspNetCore.Reporting.ReportDesigner.Native.Services;
using DevExpress.AspNetCore.Reporting.WebDocumentViewer;
using DevExpress.AspNetCore.Reporting.WebDocumentViewer.Native.Services;
using DevExpress.DataAccess.Sql;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.Web.ReportDesigner;
using Dialysis.Server.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dialysis.Server.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class CustomWebDocumentViewerController : WebDocumentViewerController
    {
        private readonly ILoggerManager _logger;
        public CustomWebDocumentViewerController(IWebDocumentViewerMvcControllerService controllerService, ILoggerManager logger) : base(controllerService)
        {
            _logger = logger;
        }
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public class CustomReportDesignerController : ReportDesignerController
    {
        private readonly ILoggerManager _logger;
        public CustomReportDesignerController(IReportDesignerMvcControllerService controllerService, ILoggerManager logger) : base(controllerService)
        {
            _logger = logger;
        }

        [HttpPost("[action]")]
        public async Task<object> GetReportDesignerModel(
            [FromForm] string reportUrl,
            [FromForm] ReportDesignerSettingsBase designerModelSettings,
            [FromServices] IReportDesignerClientSideModelGenerator designerClientSideModelGenerator)
        {
            try
            {
                Dictionary<string, object> dataSources = new Dictionary<string, object>();
                SqlDataSource ds = new SqlDataSource("ReportsDataSqlite");
                SqlDataSource ds2 = new SqlDataSource("ReportsConnectionString");
                dataSources.Add("sqlDataSource1", ds2);
                dataSources.Add("sqlDataSource2", ds);
                ReportDesignerModel model;
                if (string.IsNullOrEmpty(reportUrl))
                    model = await designerClientSideModelGenerator.GetModelAsync(new XtraReport(), dataSources, "/DXXRD", "/DXXRDV", "/DXXQB");
                else
                    model = await designerClientSideModelGenerator.GetModelAsync(reportUrl, dataSources, "/DXXRD", "/DXXRDV", "/DXXQB");
                model.WizardSettings.EnableSqlDataSource = true;
                model.Assign(designerModelSettings);
                var modelJsonScript = designerClientSideModelGenerator.GetJsonModelScript(model);
                return Content(modelJsonScript, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Content("", "application/json");
            }
        }
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public class CustomQueryBuilderController : QueryBuilderController
    {
        public CustomQueryBuilderController(IQueryBuilderMvcControllerService controllerService) : base(controllerService)
        {

        }
    }
}
