using Shipment.Entities;

namespace Shipment.Features.Shipments.Shared;

public static class ShipmentNotificationQuery
{
    public static IQueryable<ShipmentDetails> ForNotifications(
        this IQueryable<ShipmentDetails> query,
        DateTime nowLocal,
        int noticeDays,
        TimeSpan throttleInterval)
    {
        var today = nowLocal.Date;
        var windowEnd = today.AddDays(noticeDays);

        return query.Where(x =>
            x.TimeOfArrival > nowLocal &&
            x.TimeOfArrival >= today && x.TimeOfArrival <= windowEnd &&
            (x.NotifyStartAt == null || x.NotifyStartAt <= nowLocal) &&
            (x.LastNotifiedAt == null || nowLocal - x.LastNotifiedAt >= throttleInterval)
        );
    }
}