namespace APP.Eds.Models.RabbitMQ;

public class DocumentMessage
{
    public int CourtId { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string DocumentBase64 { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string ContentType { get; set; } = "application/octet-stream";
    public long FileSize { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string BusinessId { get; set; } = string.Empty;
    public string EdsId { get; set; } = string.Empty;
}