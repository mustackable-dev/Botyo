using Botyo.Contracts;
using Botyo.Entities;

namespace Botyo.Services;

public class DiscordDispatchService(IHttpClientFactory clientFactory): IDispatchService
{
    public async Task DispatchNotification(Notification notification)
    {
        using HttpClient client = clientFactory.CreateClient("DiscordClient");
        await client.PostAsJsonAsync("", new { content = notification.Content });
    }
}