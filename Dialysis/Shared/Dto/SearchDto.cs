using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dialysis.Shared.Dto
{
    public class SearchtDto
    {
        public long MedCenterId { get; set; }
        public string Inn { get; set; }
        public string Image { get; set; }
        public long Status { get; set; }
    }

    public class PatientFilterDto
    {
        public string? Inn { get; set; }
        public long? MedCenterId { get; set; } = 0;
        public string? search { get; set; }
        public DateTime? DateTimeFrom { get; set; }
        public DateTime? DateTimeTo { get; set; }
        public DateTime? BirthDateFrom { get; set; }
        public DateTime? BirthDateTo { get; set; }
        public string? Status { get; set; }
    }

    public class SessionFilterDto
    {
        public string? Inn { get; set; }
        public string? Fio { get; set; }
        public long? MedCenterId { get; set; } 
        public string? search { get; set; }
        public DateTime? DateTimeFrom { get; set; }
        public DateTime? DateTimeTo { get; set; }
        public DateTime? ChangedDateTime  { get; set; }
    }
    public class MedCentersByDistrictDto
    {
        public long RegionId { get; set; }
        public string RegionName { get; set; }
        public long TotalCount { get; set; }
    }

    public class PatientAgeGroupDto
    {
        public int Population { get; set; }
        public string AgeGroup { get; set; }
        public string Gender { get; set; }

    }
}
