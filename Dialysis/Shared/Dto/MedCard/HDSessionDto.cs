using Dialysis.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Dialysis.Shared.Dto
{
    public class HDSessionDto
    {
        public string Inn { get; set; }
        public HDSession HDSession { get; set; }
        public Patient Patient { get; set; }
        public MedCenter? MedCenter { get; set; }
        public Status? Status { get; set; }
        public MedCenterMachine? MedCenterMachine { get; set; }
        public List<HDSessionHour> HDSessionHours { get; set; }
        public List<HDSessionPause> HDSessionPauses { get; set; }
         public MedCard? MedCard { get; set; }

    }


    public class AllHDSessionDto
    {
        public long Id { get; set; }
        public string Inn { get; set; }
        public string Fio { get; set; }
        public string MedCenterTitle { get; set; }
        public string District { get; set; }
        public string Region { get; set; }
        public string Age { get; set; }
        public string MachineTitle { get; set; }
        public string StatusTitle { get; set; }
        public string GenderTitle { get; set; }
        public string? Condition { get; set; }
        public string? Complaints { get; set; }
        public string? Program { get; set; }
        public string? Dialyzer { get; set; }
        public string? Correction { get; set; }
        public string? Reinfusion { get; set; }
        public string? Access { get; set; }
        public string? Anticoagulation { get; set; }
        public string? Uf { get; set; }
        public string? Speed { get; set; }
        public string? TypeDialyzer { get; set; }
        public string? Durators { get; set; }
        public double? StartWeight { get; set; }
        public double? EndWeight { get; set; }
        public string? Note { get; set; }
        public DateTime? SessionStart { get; set; }
        public DateTime? SessionEnd { get; set; }
        public long? StatusId { get; set; }
        public double? TotalMinutes { get; set; }
        public double? TotalHours { get; set; }
        public decimal? ActivePrice { get; set; }
        public decimal? SetPrice { get; set; }
        public string? Note2 { get; set; }
        public string? Note3 { get; set; }
        public string? ImageStart { get; set; }
        public string? ImageEnd { get; set; }
        public string? Sys { get; set; }
        public string? Dia { get; set; }
        public string? Temp { get; set; }
        public string? Ritm { get; set; }

        public string? EndSys { get; set; }
        public string? EndDia { get; set; }
        public string? EndTemp { get; set; }
        public string? EndRitm { get; set; }

        public string? Sys1 { get; set; }
        public string? Dia1 { get; set; }
        public string? Temp1 { get; set; }
        public string? Ritm1 { get; set; }

        public string? Sys2 { get; set; }
        public string? Dia2 { get; set; }
        public string? Temp2 { get; set; }
        public string? Ritm2 { get; set; }

        public string? Sys3 { get; set; }
        public string? Dia3 { get; set; }
        public string? Temp3 { get; set; }
        public string? Ritm3 { get; set; }

        public string? Sys4 { get; set; }
        public string? Dia4 { get; set; }
        public string? Temp4 { get; set; }
        public string? Ritm4 { get; set; }

        public Patient Patient { get; set; }
        public List<HDSessionHour> HDSessionHours { get; set; }
        public List<HDSessionPause> HDSessionPauses { get; set; }
    }
    public class PatientSessionsDto
    {
        public Patient Patient { get; set; }
        public long Id { get; set; }
        public string Fio { get; set; }
        public string MedCenterTitle { get; set; }
        public DateTime? SessionStart { get; set; }
        public DateTime? SessionEnd { get; set; }
        public double? EffectDate { get; set; }
    }
}

