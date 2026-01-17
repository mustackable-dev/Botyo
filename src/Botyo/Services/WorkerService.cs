using System.Collections.Concurrent;
using Botyo.Contracts;
using Botyo.Extensions;
using Botyo.Models;

namespace Botyo.Services;

public class WorkerService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WorkerService> _logger;
    private CancellationTokenSource? ServiceTokenSource { get; set; }
    private Task? ServiceTask { get; set; }
    public bool IsRunning() => ServiceTask is not null;
    private SemaphoreSlim RebuildLock { get; } = new (1, 1);
    private ConcurrentDictionary<int, ScheduledNotification> Cache { get; }

    public WorkerService(
        ILogger<WorkerService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        Cache = BuildCache();
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (Cache.IsEmpty) return Task.CompletedTask;
        
        ServiceTokenSource = new();
        ServiceTask = RunScheduledNotifications(ServiceTokenSource.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (ServiceTokenSource is null) return;
        await ServiceTokenSource.CancelAsync();
        ServiceTask = null;
    }

    private async Task RunScheduledNotifications(CancellationToken cancellationToken)
    {
        do
        {
            DateTime baseTimestamp = DateTime.UtcNow;
            DateTime? earliestNotification = null;
            List<int> notificationIdsToRun = [];
            foreach (ScheduledNotification notification in Cache.Values)
            {
                if(!notification.Active) continue;
                
                DateTime? nextOccurence = notification.Schedule.GetNextOccurrence(baseTimestamp);
                if (nextOccurence is not null)
                {
                    if (earliestNotification is null || nextOccurence < earliestNotification)
                    {
                        notificationIdsToRun = [notification.Id];
                        earliestNotification = nextOccurence.Value;
                    }
                    else
                    {
                        if (nextOccurence == earliestNotification)
                        {
                            notificationIdsToRun.Add(notification.Id);
                        }
                    }
                }
            }

            TimeSpan delay = earliestNotification!.Value - DateTime.UtcNow;
            
            _logger.LogInformation("Worker service delay: {0} ms, will run tasks: {1}", delay.TotalMilliseconds,
                string.Join(',', notificationIdsToRun.Select(x => x.ToString()).ToArray()));
            
            await Task.Delay(delay, cancellationToken);
            
            using IServiceScope scope = _scopeFactory.CreateScope();
            IDispatchService dispatchService = scope.ServiceProvider.GetRequiredService<IDispatchService>();
            
            foreach (int notificationId in notificationIdsToRun)
            {
                if(Cache.TryGetValue(notificationId, out ScheduledNotification? notification))
                {
                    _ = dispatchService.DispatchNotification(notification);
                }
            }
        }
        while (!cancellationToken.IsCancellationRequested);
    }

    private ConcurrentDictionary<int, ScheduledNotification> BuildCache()
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        PersistenceService persistenceService = scope.ServiceProvider.GetRequiredService<PersistenceService>();
        IEnumerable<ScheduledNotification> notifications = persistenceService.GetNotifications()
                                                            .Result
                                                            .Select(x=>x.ToScheduledNotification());
        
        return new(notifications.Select(x => new KeyValuePair<int, ScheduledNotification>(x.Id, x)));
    }

    public async Task AddScheduledNotification(ScheduledNotification notification)
    {
        if (Cache.TryAdd(notification.Id, notification))
        {
            await RebuildWorker();
        }
    }

    public async Task ReplaceScheduledNotification(ScheduledNotification notification)
    {
        if(Cache.TryUpdate(notification.Id, notification, Cache[notification.Id]))
        {
            await RebuildWorker();
        }
    }

    public async Task DeleteScheduledNotification(int notificationId)
    {
        if (Cache.TryRemove(notificationId, out _))
        {
            await RebuildWorker();
        }
    }

    private async Task RebuildWorker()
    {
        await RebuildLock.WaitAsync();
        await StopAsync(CancellationToken.None);
        await StartAsync(CancellationToken.None);
        RebuildLock.Release();
    }
}