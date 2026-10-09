using Botyo.Configuration;
using Botyo.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Ormamu;

namespace Botyo.Services;

public class PersistenceService(IOptions<PersistenceConfig> config)
{
    private SqliteConnection GetConnection()
        => new (config.Value.ConnectionString);

    public async Task<Notification> CreateNotificationAsync(Notification notification)
        => notification with { Id = await GetConnection().InsertAsync(notification) };

    public async Task<Notification> UpdateNotificationAsync(Notification notification)
    {
        await GetConnection().UpdateAsync(notification);
        return notification;
    }

    public Task<Notification?> GetNotificationAsync(int id)
        => GetConnection().GetAsync<Notification?>(id);

    public Task<IEnumerable<Notification>> GetNotificationsAsync()
        => GetConnection().GetAsync<Notification>();

    public Task DeleteNotificationAsync(int id)
        => GetConnection().DeleteAsync<Notification>(id);
}