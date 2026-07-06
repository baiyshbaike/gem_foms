using Dialysis.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dialysis.Shared.Params
{
    public class DynamicAnalysis
    {
        public long Id { get; set; }
        public long MedCardId { get; set; }
        public long? PatientId { get; set; }
        public long AnalysisId { get; set; }
        public long? AnalysisResultGroupId { get; set; }
        public string? AnalysisName { get; set; }
        public string Result { get; set; }
        public string? Status { get; set; }
        public string? Note { get; set; }
        public Analysis Analysis { get; set; }
        public DateTime? AnalysisDate { get; set; }

    }
}