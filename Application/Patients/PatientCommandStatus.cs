namespace Application.Patients;

public enum PatientCommandStatus
{
    Succeeded = 1,
    NotFound = 2,
    Conflict = 3,
    ValidationFailed = 4
}
