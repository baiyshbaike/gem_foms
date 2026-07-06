using Dialysis.Shared.Models;

namespace Dialysis.Shared.Dto;

public class PagedResponseDto<T>
{
    public List<T> Items { get; set; }
    public int TotalItems { get; set; }
    public PatientCountDto PatientCount { get; set; }
}

public class SessionResponseDto<T>
{
    public List<T> Items { get; set; }
    public int TotalItems { get; set; }
    public SessionCountDto  SessionCount { get; set; }
    public bool? IsSuccess  { get; set; }
    public string? Message { get; set; }
}
public class FilesResponseDto<T>
{
    public List<T> Items { get; set; }
    public MedCenter? MedCenter { get; set; }
    public User? User { get; set; }
}