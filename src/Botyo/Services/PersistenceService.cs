using System.Data;
using Cronos;
using Botyo.Entities;
using Botyo.Models;
using Microsoft.Data.Sqlite;
using Ormamu;

namespace Botyo.Services;

public class PersistenceService(ILogger<PersistenceService> logger)
{
    public static string ConnectionString { get; set; } = null!;
    private static IDbConnection GetConnection()
        => new SqliteConnection(ConnectionString);

    public async Task<Notification> CreateNotification(Notification notification)
        => notification with { Id = await GetConnection().InsertAsync(notification) };

    public async Task<Notification> UpdateNotification(Notification notification)
    {
        await GetConnection().UpdateAsync(notification);
        return notification;
    }

    public Task<Notification?> GetNotification(int id)
        => GetConnection().GetAsync<Notification?>(id);

    public Task<IEnumerable<Notification>> GetNotifications()
        => GetConnection().GetAsync<Notification>();

    public Task DeleteNotification(int id)
        => GetConnection().DeleteAsync<Notification>(id);
}