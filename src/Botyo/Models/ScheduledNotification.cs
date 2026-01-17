using Cronos;
using Botyo.Entities;

namespace Botyo.Models;

public record ScheduledNotification : Notification
{
    public required CronExpression Schedule { get; init; }
}