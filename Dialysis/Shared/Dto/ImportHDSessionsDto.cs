using Dialysis.Shared.Models;

namespace Dialysis.Shared.Dto;

public class ImportHDSessionsDto
{
    public HDSession HdSession { get; set; }
    public List<HDSessionHour> HdSessionHours { get; set; } = new List<HDSessionHour>();
}