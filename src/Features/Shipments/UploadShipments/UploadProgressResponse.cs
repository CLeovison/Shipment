namespace Shipment.Features.Shipments.UploadShipments;

public sealed class UploadProgressResponse
{
    public Guid UploadId { get; set; }
    public int Total { get; set; }
    public int Processed { get; set; }
    public int Succeeded { get; set; }
    public int Failed { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
