using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dialysis.Shared.Dto
{
   
    public class PatientRepDto
    {
        public string Fio { get; set; }
        public string Inn { get; set; }
        public string MedCenterTitle { get; set; }
        public int? TotalHDSessions { get; set; }
    }
    
    public class PatientSessionsRepDto
    {
        public string Fio { get; set; }
        public string Inn { get; set; }
        public int? TotalHDSessions { get; set; }
    }
}