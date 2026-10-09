namespace Botyo.Models;

public record NotificationUpdateRequest: NotificationCreateRequest
{
    public bool Active {get; init;}
}
