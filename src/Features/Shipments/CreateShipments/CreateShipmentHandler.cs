using Shipment.Abstract;
using Shipment.Abstract.Results;
using Shipment.Database;
using Shipment.Entities;

namespace Shipment.Features.Shipments.CreateShipments;

internal sealed class CreateShipmentHandler(AppDbContext dbContext)
{
    public async Task<Result<ShipmentDetails>> CreateShipmentAsync(ShipmentDetails shipments, CancellationToken ct)
    {


        return shipments;
    }
}



public sealed class CreateShipmentEndpoint : IEndpoint
{
    public void Endpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("", async () =>
        {

        });
    }
}