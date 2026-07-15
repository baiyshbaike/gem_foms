using System.ComponentModel.DataAnnotations;

namespace Contracts.Patients;

public sealed class PatientGridExportRequest : PatientGridQueryRequest
{
    [MaxLength(50_000)]
    public long[] SelectedIds { get; set; } = [];
}
