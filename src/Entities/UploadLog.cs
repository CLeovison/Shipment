namespace Shipment.Entities;

public sealed class UploadLog
{
    public Guid Id { get; set; }
    public int UserId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Succeeded { get; set; }
    public int Failed { get; set; }
    public int Processed { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
