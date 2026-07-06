using Dialysis.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dialysis.Shared.Params
{
    public class IndicatorPostArgs
    {
        public long Id { get; set; }
        public DateTime? startIndicator { get; set; }
        public DateTime? endIndicator { get; set; }
        public long MedCenterId { get; set; }
        public string? Title { get; set; }
        public DateTime? CreatedOn { get; set; }
        public List<IndicatorRow> IndicatorRows { get; set; } = new List<IndicatorRow>();
    }

}
