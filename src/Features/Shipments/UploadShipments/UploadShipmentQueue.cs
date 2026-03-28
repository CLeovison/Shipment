using System.Threading.Channels;
using Shipment.Entities;

namespace Shipment.Features.Shipments.UploadShipments;

public sealed class UploadShipmentQueue
{
    private readonly Channel<ShipmentImportDto> channel = Channel.CreateBounded<ShipmentImportDto>(new BoundedChannelOptions(1000)
    {
        SingleReader = true,
        SingleWriter = false,
        FullMode = BoundedChannelFullMode.Wait

    });
    public ChannelWriter<ShipmentImportDto> Writer => channel.Writer;
    public ChannelReader<ShipmentImportDto> Reader => channel.Reader;
}