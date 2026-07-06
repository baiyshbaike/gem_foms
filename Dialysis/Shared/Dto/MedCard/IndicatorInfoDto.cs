using Dialysis.Shared.Models;

namespace Dialysis.Shared.Dto;

public class IndicatorInfoDto
{
    public List<Patient> Patients { get; set; }
    public List<Patient> PatientsOfWeek { get; set; }
}