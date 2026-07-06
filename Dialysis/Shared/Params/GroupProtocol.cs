using Dialysis.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dialysis.Shared.Params
{
    public class GroupProtocolArgs
    {
        public PatientGroupTitle PatientGroupTitle { get; set; }
        public List<PatientHistory> PatientHistory { get; set; }
        public List<PatientGroupPerson> PatientGroupPerson { get; set; }
    }

    public class GroupProtocolParams
    {
        public string? Description { get; set; }
        public long PatientId { get; set; }
        public long GroupId { get; set; }
        public long GroupTitleId { get; set; }
        public long? GroupFromId { get; set; }
        public string? GroupText { get; set; }       
        public long? GroupReasonId { get; set; }
        public long? GroupLPUId { get; set; }
    }


}
