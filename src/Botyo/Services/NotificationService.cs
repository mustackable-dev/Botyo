using Botyo.Contracts;
using Cronos;
using Botyo.Entities;
using Botyo.Extensions;
using Botyo.Models;

namespace Botyo.Services;

public class NotificationService(
    PersistenceService persistence,
    WorkerService workerService,
    IDispatchService dispatchService)
{
    public async Task<Result<Notification>> CreateNotificationAsync(NotificationUpsertRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            CronExpression.Parse(request.Cron);
        }
        catch (CronFormatException exception)
        {
            return Result<Notification>.Failure(StatusCodes.Status400BadRequest, exception);
        }
        
        Notification persistedNotification = await persistence.CreateNotificationAsync(new ()
        {
            Active = true,
            WorkdayOnly = request.WorkdayOnly,
            Content = request.Content,
            Cron = request.Cron
        });
        
        await workerService.AddScheduledNotificationAsync(persistedNotification.ToScheduledNotification(), cancellationToken);
        
        return Result<Notification>.Success(StatusCodes.Status201Created, persistedNotification);
    }

    public async Task<Result<Notification>> GetNotificationAsync(int id)
    {
        Notification? notification = await persistence.GetNotificationAsync(id);
        
        if(notification is null)
            return Result<Notification>.Failure(
                StatusCodes.Status404NotFound,
                new KeyNotFoundException($"Notification with id {id} not found"));
        
        return Result<Notification>.Success(StatusCodes.Status200OK, notification);
    }

    public async Task<Result<IEnumerable<Notification>>> GetAllNotificationsAsync()
        => Result<IEnumerable<Notification>>.Success(StatusCodes.Status200OK, await persistence.GetNotificationsAsync());
    
    public async Task<Result<Notification>> UpdateNotificationAsync(int id, NotificationUpsertRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            CronExpression.Parse(request.Cron);
        }
        catch (CronFormatException exception)
        {
            return Result<Notification>.Failure(StatusCodes.Status400BadRequest, exception);
        }
        
        Result<Notification> notification = await GetNotificationAsync(id);
        if(notification.Error is not null)
            return notification;
        
        Notification updatedNotification = await persistence.UpdateNotificationAsync(new ()
        {
            Id = id,
            WorkdayOnly = request.WorkdayOnly,
            Active = request.Active,
            Content = request.Content,
            Cron = request.Cron
        });
        
        await workerService.ReplaceScheduledNotificationAsync(updatedNotification.ToScheduledNotification(), cancellationToken);
        
        return Result<Notification>.Success(StatusCodes.Status200OK, updatedNotification);
    }

    public async Task<Result<bool>> ChangeNotificationStatusAsync(int id, SetBooleanRequest request, CancellationToken cancellationToken = default)
    {
        Result<Notification> notification = await GetNotificationAsync(id);
        if(notification.Error is not null)
            return Result<bool>.Failure(notification.StatusCode, notification.Error);

        Notification updatedNotification = notification.Payload! with { Active = request.Value };
        
        await persistence.UpdateNotificationAsync(updatedNotification);
        await workerService.ReplaceScheduledNotificationAsync(updatedNotification.ToScheduledNotification(), cancellationToken);
        
        return Result<bool>.Success(StatusCodes.Status200OK, true);
    }

    public async Task<Result<bool>> DeleteNotificationAsync(int id, CancellationToken cancellationToken = default)
    {
        Result<Notification> notification = await GetNotificationAsync(id);
        if(notification.Error is not null)
            return Result<bool>.Failure(notification.StatusCode, notification.Error);
        
        await persistence.DeleteNotificationAsync(id);
        await workerService.DeleteScheduledNotificationAsync(id, cancellationToken);
        
        return Result<bool>.Success(StatusCodes.Status204NoContent, true);
    }

    public async Task<Result<bool>> RunNotificationManuallyAsync(int id, CancellationToken cancellationToken = default)
    {
        Result<Notification> notification = await GetNotificationAsync(id);
        if(notification.Error is not null)
            return Result<bool>.Failure(notification.StatusCode, notification.Error);
        
        await dispatchService.DispatchNotificationAsync(notification.Payload!, cancellationToken);
        return Result<bool>.Success(StatusCodes.Status200OK, true);
    }
}

