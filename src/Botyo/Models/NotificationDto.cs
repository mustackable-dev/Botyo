using System.ComponentModel.DataAnnotations;

namespace Botyo.Models;

public record NotificationDto
{
    [Length(1, 9000)]
    public required string Content { get; init; }
    [Length(9, 100)]
    public required string Cron {get; init;}
}

