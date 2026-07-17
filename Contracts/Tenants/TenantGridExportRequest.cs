using System.ComponentModel.DataAnnotations;

namespace Contracts.Tenants;

public sealed class TenantGridExportRequest : TenantGridQueryRequest
{
    [MaxLength(50_000)]
    public string[] SelectedIds { get; set; } = [];
}
