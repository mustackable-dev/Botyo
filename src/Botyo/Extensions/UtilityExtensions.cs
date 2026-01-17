using Botyo.Entities;
using Botyo.Models;
using Cronos;

namespace Botyo.Extensions;

public static class UtilityExtensions
{
    public static ScheduledNotification ToScheduledNotification(this Notification notification)
        => new()
        {
            Id = notification.Id,
            Content = notification.Content,
            Cron = notification.Cron,
            Active = notification.Active,
            Schedule = CronExpression.Parse(notification.Cron)
        };
}