using System.ComponentModel.DataAnnotations;

namespace Botyo.Models;

public record NotificationUpdateDto: NotificationDto
{
    public bool Active {get; init;}
}
