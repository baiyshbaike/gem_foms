using System.ComponentModel.DataAnnotations;

namespace Contracts.Sessions;

public sealed class UpdateSessionWorkflowSettingsRequest
{
    [Range(1, 1440)]
    public int IdentificationStartLimitMinutes { get; set; }

    [Range(1, 1440)]
    public int AutoFinishActiveMinutes { get; set; }

    [Range(1, 1440)]
    public int EndIdentificationLimitMinutes { get; set; }

    [Range(1, 1440)]
    public int SendToPayLimitMinutes { get; set; }
}
