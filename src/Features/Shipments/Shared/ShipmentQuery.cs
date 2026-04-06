using Shipment.Entities;

namespace Shipment.Features.Shipments.Shared;

public static class ShipmentNotificationQuery
{
    public static IQueryable<ShipmentDetails> ForNotifications(
        this IQueryable<ShipmentDetails> query,
        DateTime nowUtc,
        int noticeDays,
        TimeSpan throttleInterval)
    {
        var windowEnd = nowUtc.Date.AddDays(noticeDays);

        return query.Where(x =>
            // 1. Not completed
            !x.IsCompleted &&

            // 2. Within arrival window (date-based)
            x.TimeOfArrival >= nowUtc.Date &&
            x.TimeOfArrival <= windowEnd &&

            // 3. Notification has started
            (
                x.NotifyStartAt == null ||
                x.NotifyStartAt <= nowUtc
            ) &&

            // 4. Throttle (critical)
            (
                x.LastNotifiedAt == null ||
                nowUtc - x.LastNotifiedAt >= throttleInterval
            )
        );
    }
}