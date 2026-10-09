using Cronos;
using Botyo.Entities;

namespace Botyo.Models;

public record ScheduledNotification : Notification
{
    public required CronExpression Schedule { get; init; }
    public Queue<DateTimeOffset> OccurenceQueue { get; init; } = new(1);
}