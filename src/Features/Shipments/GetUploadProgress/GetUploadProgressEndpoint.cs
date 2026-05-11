using Shipment.Abstract;
using Shipment.Database;
using Shipment.Features.Shipments.Shared;

namespace Shipment.Features.Shipments.UploadShipments;

public sealed class GetUploadProgressEndpoint : IEndpoint
{
    public void Endpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/shipments/upload/{uploadId:guid}/progress", async (Guid uploadId, UploadProgressStore progressStore, AppDbContext context) =>
        {
            // Try in-memory first (active upload)
            var progress = progressStore.GetId(uploadId);

            if (progress is not null)
            {
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
            }

            // Fall back to database (completed/historical upload)
            var log = await context.UploadLogs.FindAsync(uploadId);

            if (log is null)
                return Results.NotFound(new { Message = "Upload not found" });

            return Results.Ok(new
            {
                UploadId = log.Id,
                log.Total,
                log.Processed,
                log.Succeeded,
                log.Failed,
                log.IsCompleted,
                log.StartedAt,
                log.CompletedAt
            });
        })
        .RequireAuthorization();
    }
}
