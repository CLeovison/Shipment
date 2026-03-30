namespace Shipment.Entities.Shared;

public static class ShipmentNotificationQuery
{
    public static IQueryable<ShipmentDetails> ForNotifications(
        this IQueryable<ShipmentDetails> query,
        DateTime todayUtc,
        int noticeDays)
    {
        var windowEnd = todayUtc.AddDays(noticeDays);

        return query.Where(x =>
            !x.IsCompleted &&
            (x.NotifyStartAt == null || x.NotifyStartAt <= todayUtc) &&
            x.TimeOfArrival >= todayUtc &&
            x.TimeOfArrival <= windowEnd &&
            (x.LastNotifiedAt == null || x.LastNotifiedAt < todayUtc.Date));
    }
}