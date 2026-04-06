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
            // 1. NOT completed (derived, NOT stored)
            x.TimeOfArrival > nowLocal &&

            // 2. Within window
            x.TimeOfArrival >= today &&
            x.TimeOfArrival <= windowEnd &&

            // 3. Notification start
            (
                x.NotifyStartAt == null ||
                x.NotifyStartAt <= nowLocal
            ) &&

            // 4. Throttle
            (
                x.LastNotifiedAt == null ||
                nowLocal - x.LastNotifiedAt >= throttleInterval
            )
        );
    }
}