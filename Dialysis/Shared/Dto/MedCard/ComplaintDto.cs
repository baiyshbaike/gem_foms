using Dialysis.Shared.Models;

namespace Dialysis.Shared.Dto;

public class ComplaintDto
{
    public Complaint Complaint { get; set; }
    public Patient Patient { get; set; }
    public MedCenter MedCenter { get; set; }
}