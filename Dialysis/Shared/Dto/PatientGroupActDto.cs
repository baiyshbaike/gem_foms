using Dialysis.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dialysis.Shared.Dto
{
    public class PatientGroupActDto
    {
        public PatientGroupTitle PatientGroupTitle { get; set; }
        public List<GroupActWithPatient> PatientHistory { get; set; }      
        public List<PatientGroupPersonDto> PersonGroup { get; set; }
    }

    public class GroupActWithPatient
    {
        public long Id { get; set; }
        public long GroupTitleId { get; set; }
        public long GroupId { get; set; }
        public string? GroupText { get; set; }
        public long? GroupFromId { get; set; }
        public string? GroupFromText { get; set; }
        public long PatientId { get; set; }
        public string? Inn { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MiddleName { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ActDate { get; set; }

        public long? GroupReasonId { get; set; }
        public long? GroupLPUId { get; set; }
        public string? ReasonText { get; set; }
        public string? LPUText { get; set; }
    }

    public class PatientGroupPersonDto
    {
        public long Id { get; set; }
        public long GroupTitleId { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public long PersonTitleId { get; set; }
        public string? PersonTitleText { get; set; }
        public string? PersonFio { get; set; }
    }

    public class PatientHistoryDto
    {
        public DateTime AcDate { get; set; }
        public List<PatientGroup> PatientGroups { get; set; }
        public List<Patient> Patients { get; set; }
    }

    public class PatientChangesDto
    {
        public long Id { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? Before { get; set; }
        public int? Come { get; set; }
        public int? Gone { get; set; }
        public int? After { get; set; }
        public List<GroupActWithPatient> ComeList { get; set; }
        public List<GroupActWithPatient> GoneList { get; set; }
    }
    public class PatientInfo
    {
        public string Pin { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public bool Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Policy { get; set; }
        public int Mhi { get; set; }
        public DateTime DateBegin { get; set; }
        public DateTime DateEnd { get; set; }
        public string Category { get; set; }
        public int Citizenship { get; set; }
        public bool IsPayed { get; set; }
    }
}
