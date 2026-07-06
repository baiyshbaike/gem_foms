using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Dialysis.Shared.Models.Files;
using System.ComponentModel.DataAnnotations;
namespace Dialysis.Shared.Models
{

    public class PatientTotalHdDto
    {
        public long Id { get; set; }
        public string Fio { get; set; }
        public string Inn { get; set; }
        public int? Total { get; set; }
        public DateTime? LastDate { get; set; }
        public string? PhoneNumber1 { get; set; }
        public string? PhoneNumber2 { get; set; }
        public string? MedCenterTitle { get; set; }
    }

    public class PatientTotalMedCenterDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public long MedCenterId { get; set; }
        public int? TotalPatient { get; set; }        
        public string Fio { get; set; }
        public string Inn { get; set; }
        public int? Total { get; set; }
        public DateTime? LastDate { get; set; }
    }
    public class PatientBoxDto
    {
        public long Id { get; set; }
        public string Inn { get; set; }
        public string TextValue { get; set; }       
        public long? GroupId { get; set; }        
    }
    public class PatientResultDto
    {
        public Patient Patient { get; set; }
        public int Status { get; set; }
        public string Err { get; set; }
        public string Message { get; set; }
        public string Result { get; set; }
    }
    public class PatientSearchDto
    {
        [Required(ErrorMessage = "Введите идентификационный номер")]
        [StringLength(14, MinimumLength = 14, ErrorMessage = "Идентификационный номер должен состоять из 14 символов!")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Идентификационный номер должен содержать только цифры!")]
        public string Inn { get; set; }
    
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
    
    public class SessionCountDto
    {
        public int All { get; set; }
        public int IdentificationCount { get; set; }
        public int ActiveCount { get; set; }
        public int FinishedCount { get; set; }
        public int EndIdentificationCount { get; set; }
        public int SendToPayCount { get; set; }
        public int PayedCount { get; set; }
        public int OtherCount { get; set; }
    }
    public class PatientCountDto
    {
        public int All { get; set; }
        public int NewCount { get; set; }
        public int ArchiveCount { get; set; }
        public int FomsCount { get; set; }
        public int FreseniusCount { get; set; }
        public int OtherCount { get; set; }
    }
    public class PatientAddEditDto
    {
        public long Id { get; set; }
        [Required(ErrorMessage = "Идентификационный номер обязателен!")]
        [StringLength(14, ErrorMessage = "Идентификационный номер должен состоять из 14 символов!", MinimumLength = 14)]
        [RegularExpression(@"^\d+$", ErrorMessage = "Идентификационный номер должен содержать только цифры!")]
        public string Inn { get; set; }
        [Required(ErrorMessage = "Имя обязательно!")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Фамилия обязательна!")]
        public string LastName { get; set; }
        public string? MiddleName { get; set; }
        public string? Address { get; set; }
        public string? Address2 { get; set; }
        public string? Phone { get; set; }
        public string? Phone2 { get; set; }  
        [Required(ErrorMessage = "Область обязательна!")]
        public long DistrictId { get; set; }
        [Required(ErrorMessage = "Регион обязателен!")]
        public long RegionId { get; set; }
        public long? MedCenterId { get; set; }
        public string? Image { get; set;}
        [Required(ErrorMessage = "Пол пациента обязателен!")]
        public bool Gender { get; set; }
        [Required(ErrorMessage = "Дата рождения обязательна!")]
        public DateTime BirthDate { get; set; }
        public string? File1 { get; set;}
        public string? File2 { get; set;}
        public string? File3 { get; set;}
        public string? File4 { get; set;}
        public bool SpecialStatus { get; set; }
    }

    public class PatientInfoDto
    {
        public Patient Patient { get; set; } = new Patient();
        public IEnumerable<MedCard> MedCards { get; set; } = new List<MedCard>();
        public IEnumerable<HDSession> HdSessions { get; set; } = new List<HDSession>();
        public IEnumerable<SaveFile> SaveFiles { get; set; } = new List<SaveFile>();
        public IEnumerable<AnalysisResult> AnalysisResults { get; set; } = new List<AnalysisResult>();
    }
    public class DializDto
    {
        public string Title { get; set; }
        public string Phone { get; set; }
        public string Phone2 { get; set; }    
        public string Adress { get; set; }
        public string Adress2 { get; set; }
        public int Total { get; set; }
    }

    public class ProcessDto
    {
        public string Fio { get; set; }
        public string Inn { get; set; }
        public string Title { get; set; }
        public string Start { get; set; }
        public string End { get; set; }       
        public double Hours { get; set; }        
    }

    
}
