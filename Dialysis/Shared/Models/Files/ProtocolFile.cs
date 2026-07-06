namespace Dialysis.Shared.Models.Files;

public class ProtocolFile
{
    public long Id { get; set; }
    public string? Inn { get; set; }
    public long? EntityId { get; set; }
    public string ContentType { get; set; }
    public string Base64File { get; set; }
    public string FileName { get; set; }
    public byte[] Data { get; set; }
    public DateTime CreatedOn { get; set; }
    public long? CreatedBy { get; set; }
}