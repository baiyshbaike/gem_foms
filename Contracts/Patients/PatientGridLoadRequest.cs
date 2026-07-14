namespace Contracts.Patients;

public class PatientGridLoadRequest
{
    public int Skip { get; set; }
    public int Take { get; set; } = 25;
    public bool RequireTotalCount { get; set; } = true;
    public bool RequireGroupCount { get; set; }
    public bool IsCountQuery { get; set; }
    public string? Sort { get; set; }
    public string? Group { get; set; }
    public string? Filter { get; set; }
    public string? TotalSummary { get; set; }
    public string? GroupSummary { get; set; }
}
