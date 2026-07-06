using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dialysis.Shared.Models
{

    public class AccountRegistryDto
    {
        public int? OrderNo { get; set; }
        public string Inn { get; set; }
        public string Fio { get; set; }
        public int CountHd { get; set; }
        public decimal TotalPrice { get; set; }
        public long MedcenterId { get; set; }
    }
    
    public class AccountRegMedCenterDto
    {
        public long MedCenterId { get; set; }
        public string MedCenterTitle { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal ActivePrice { get; set; }
        public List<AccountRegistryDto> AccountRegistries { get; set; }
    }
    public class RegMedCenterOblDto
    {
        public int? OrderNo { get; set; }
        public long MedCenterId { get; set; }
        public string MedCenterTitle { get; set; }
        public int SessionCount { get; set; }
        public int PatientCount { get; set; }
        public decimal TotalPrice { get; set; }
        public long OblId { get; set;}
        public string Inn { get; set; }
    }

    public class RegOblDto
    {
        public long OblId { get; set; }
        public string OblTitle { get; set; }
        public decimal ActivePrice { get; set; }
        public List<RegMedCenterOblDto> RegMedCenterOblDtos { get; set; }
    }
}
