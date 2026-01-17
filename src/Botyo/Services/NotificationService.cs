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
    public async Task<Result<Notification>> CreateNotification(NotificationDto notification)
    {
        try
        {
            CronExpression.Parse(notification.Cron);
        }
        catch (CronFormatException exception)
        {
            return Result<Notification>.Failure(StatusCodes.Status400BadRequest, exception);
        }
        
        Notification persistedNotification = await persistence.CreateNotification(new ()
        {
            Active = true,
            Content = notification.Content,
            Cron = notification.Cron
        });
        
        await workerService.AddScheduledNotification(persistedNotification.ToScheduledNotification());
        
        return Result<Notification>.Success(StatusCodes.Status201Created, persistedNotification);
    }

    public async Task<Result<Notification>> GetNotification(int id)
    {
        Notification? notification = await persistence.GetNotification(id);
        
        if(notification is null)
            return Result<Notification>.Failure(
                StatusCodes.Status404NotFound,
                new KeyNotFoundException($"Notification with id {id} not found"));
        
        return Result<Notification>.Success(StatusCodes.Status200OK, notification);
    }

    public async Task<Result<IEnumerable<Notification>>> GetAllNotifications()
        => Result<IEnumerable<Notification>>.Success(StatusCodes.Status200OK, await persistence.GetNotifications());
    
    public async Task<Result<Notification>> UpdateNotification(int id, NotificationUpdateDto notificationUpdate)
    {
        try
        {
            CronExpression.Parse(notificationUpdate.Cron);
        }
        catch (CronFormatException exception)
        {
            return Result<Notification>.Failure(StatusCodes.Status400BadRequest, exception);
        }
        
        Result<Notification> notification = await GetNotification(id);
        if(notification.Error is not null)
            return notification;
        
        Notification updatedNotification = await persistence.UpdateNotification(new ()
        {
            Id = id,
            Active = notificationUpdate.Active,
            Content = notificationUpdate.Content,
            Cron = notificationUpdate.Cron
        });
        
        await workerService.ReplaceScheduledNotification(updatedNotification.ToScheduledNotification());
        
        return Result<Notification>.Success(StatusCodes.Status200OK, updatedNotification);
    }

    public async Task<Result<bool>> ChangeNotificationStatus(int id, bool active)
    {
        Result<Notification> notification = await GetNotification(id);
        if(notification.Error is not null)
            return Result<bool>.Failure(notification.StatusCode, notification.Error);

        Notification updatedNotification = notification.Payload! with { Active = active };
        
        await persistence.UpdateNotification(updatedNotification);
        await workerService.ReplaceScheduledNotification(updatedNotification.ToScheduledNotification());
        
        return Result<bool>.Success(StatusCodes.Status200OK, true);
    }

    public async Task<Result<bool>> DeleteNotification(int id)
    {
        Result<Notification> notification = await GetNotification(id);
        if(notification.Error is not null)
            return Result<bool>.Failure(notification.StatusCode, notification.Error);
        
        await persistence.DeleteNotification(id);
        await workerService.DeleteScheduledNotification(id);
        
        return Result<bool>.Success(StatusCodes.Status204NoContent, true);
    }

    public async Task<Result<bool>> RunNotificationManually(int id)
    {
        Result<Notification> notification = await GetNotification(id);
        if(notification.Error is not null)
            return Result<bool>.Failure(notification.StatusCode, notification.Error);
        
        await dispatchService.DispatchNotification(notification.Payload!);
        return Result<bool>.Success(StatusCodes.Status200OK, true);
    }
}

