using DevExpress.XtraReports.UI;

namespace Dialysis.Server.Data
{
    public class StaticReportsFactory
    {
        public static Dictionary<string, Func<XtraReport>> Reports = new Dictionary<string, Func<XtraReport>>()
        {
            ["MCenterSessions"] = () => new MCenterSessions()
        };
    }
}
