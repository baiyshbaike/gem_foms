namespace Dialysis.Shared.Dto;

public class ConfirmDto
{
    public string SessionId { get; set; }
    public string SecretKey { get; set; }
    public bool Status { get; set; }
}
public class RequestVerificationDto { }

public class RequestVerificationResponseDto
{
    public string SessionId { get; set; }
    public DateTime Expires { get; set; }
    public string SecretKey { get; set; }
}

public class RequestPatientSessionDto
{
    public string Inn { get; set; }
    public string MedCenterTitle { get; set; }
    public DateTime? Date { get; set; }
    public DateTime? StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
}