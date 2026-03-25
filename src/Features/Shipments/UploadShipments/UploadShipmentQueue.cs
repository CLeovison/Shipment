using System.Threading.Channels;
using Shipment.Entities;

namespace Shipment.Features.Shipments.UploadShipments;

public sealed class UploadShipmentQueue
{
    private readonly Channel<ShipmentDetails> channel = Channel.CreateBounded<ShipmentDetails>(new BoundedChannelOptions(1000)
    {
        SingleReader = true,
        SingleWriter = false,
        FullMode = BoundedChannelFullMode.Wait

    });
    public ChannelWriter<ShipmentDetails> Writer => channel.Writer;
    public ChannelReader<ShipmentDetails> Reader => channel.Reader;
}