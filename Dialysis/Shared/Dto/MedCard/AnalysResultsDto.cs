using Dialysis.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dialysis.Shared.Dto
{
    public class AnalysisResultsDto
    {
        public Patient Patient { get; set; }
        public AnalysisResult AnalysisResult { get; set; }
        public Analysis Analysis { get; set; }
        public MedCard MedCard { get; set; }
        public MedCenter MedCenter { get; set; }
    }
}
