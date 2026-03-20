using System.Threading.Channels;

namespace Shipment.Features.Shipments.UploadShipments;

public sealed class UploadShipmentQueue(Channel<ShipmentCsvRecord> channel)
{
    public ChannelWriter<ShipmentCsvRecord> channelWriter => channel.Writer;
    public ChannelReader<ShipmentCsvRecord> channelReader => channel.Reader;

    public static UploadShipmentQueue CreateUnbounded() => new UploadShipmentQueue(Channel.CreateUnbounded<ShipmentCsvRecord>());
    public static UploadShipmentQueue CreateBounded(int capacity) => new UploadShipmentQueue(Channel.CreateBounded<ShipmentCsvRecord>(
        new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        }));
}