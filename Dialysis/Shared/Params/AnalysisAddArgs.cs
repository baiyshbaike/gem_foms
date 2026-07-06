using Dialysis.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dialysis.Shared.Params
{
    public class AnalysisAddArgs
    {
        public string Inn { get; set; }
        public List<AnalysisResult> AnalysisResults { get; set; }
    }
}
