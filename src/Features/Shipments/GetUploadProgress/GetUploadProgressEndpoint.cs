using Shipment.Abstract;
using Shipment.Features.Shipments.Shared;

namespace Shipment.Features.Shipments.UploadShipments;

public sealed class GetUploadProgressEndpoint : IEndpoint
{
    public void Endpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/shipments/upload/{uploadId:guid}/progress", (Guid uploadId, UploadProgressStore progressStore) =>
        {
            var progress = progressStore.GetId(uploadId);

            if (progress is null)
                return Results.NotFound(new { Message = "Upload not found" });

            return Results.Ok(new
            {
                progress.UploadId,
                progress.Total,
                progress.Processed,
                progress.Succeeded,
                progress.Failed,
                progress.IsCompleted,
                progress.StartedAt,
                progress.CompletedAt
            });
        })
        .RequireAuthorization();
    }
}
