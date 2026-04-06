namespace Shipment.Features.Shipments.Shared;

public static class DateHelper
{
    public static DateTime ToUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    public static DateTime ToUtcDate(DateTime? value)
    {
        if (value is null)
            return DateTime.UtcNow.Date;

        return DateTime.SpecifyKind(value.Value.Date, DateTimeKind.Utc);
    }
}