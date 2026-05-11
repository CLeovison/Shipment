namespace Shipment.Features.Shipments.Shared;

public sealed class UploadProgress
{
    public Guid UploadId { get; set; }
    public int Succeeded { get; set; }
    public int Failed { get; set; }
    public int Total { get; set; }
    public int Processed { get; set; }
    public bool IsCompleted { get; set; }

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public List<string> Errors { get; set; } = new();
}