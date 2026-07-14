namespace Contracts.Patients;

public sealed class PatientGridExportRequest : PatientGridLoadRequest
{
    public long[] SelectedIds { get; set; } = [];
}
