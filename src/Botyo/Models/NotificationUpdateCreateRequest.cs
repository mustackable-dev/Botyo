namespace Botyo.Models;

public record NotificationUpdateCreateRequest: NotificationCreateRequest
{
    public bool Active {get; init;}
}
