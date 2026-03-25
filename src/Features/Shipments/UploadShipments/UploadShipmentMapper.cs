using CsvHelper.Configuration;

namespace Shipment.Features.Shipments.UploadShipments;

public sealed class ShipmentCsvRecordMap : ClassMap<ShipmentCsvRecord>
{
    public ShipmentCsvRecordMap()
    {
        Map(m => m.PurchaseOrderNumber).Name("PurchaseOrderNumber");
        Map(m => m.Vendor).Name("Vendor");
        Map(m => m.TimeOfArrival).Name("TimeOfArrival");
    }
}