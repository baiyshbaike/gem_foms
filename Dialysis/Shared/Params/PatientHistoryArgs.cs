using Dialysis.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dialysis.Shared.Params
{
    public class PatientHistoryArgs
    {
        public long PatientId { get; set; }
        public long GroupId { get; set; }
        public DateTime? ActDate { get; set; }
    }
}
