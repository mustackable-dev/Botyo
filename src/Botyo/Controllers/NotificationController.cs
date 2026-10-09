using Botyo.Entities;
using Botyo.Models;
using Botyo.Services;
using Microsoft.AspNetCore.Mvc;

namespace Botyo.Controllers;

[Route("Notifications")]
public class NotificationController(
    NotificationService notificationService,
    WorkerService workerService) : BaseController
{
    [HttpPost]
    [ProducesResponseType<Notification>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateNotification(NotificationCreateRequest notification, CancellationToken cancellationToken = default)
    {
        Result<Notification> result = await notificationService.CreateNotificationAsync(notification, cancellationToken);

        if (!workerService.IsRunning())
        {
            await workerService.StartAsync(CancellationToken.None);
        }
        
        return FromResult(result);
    }
    
    [HttpPost("{id:int}/Run")]
    [ProducesResponseType<Notification>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RunNotification(int id, CancellationToken cancellationToken=default)
        => FromResult(await notificationService.RunNotificationManuallyAsync(id, cancellationToken));
    
    [HttpPut("{id:int}")]
    [ProducesResponseType<Notification>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateNotification(int id, NotificationUpdateRequest notification, CancellationToken cancellationToken=default)
        => FromResult(await notificationService.UpdateNotificationAsync(id, notification, cancellationToken));

    [HttpGet]
    [ProducesResponseType<Notification>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllNotifications()
        => FromResult(await notificationService.GetAllNotificationsAsync());
    
    [HttpGet("{id:int}")]
    [ProducesResponseType<Notification>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNotification(int id)
        => FromResult(await notificationService.GetNotificationAsync(id));
    
    [HttpPatch("{id:int}/active")]
    [ProducesResponseType<Notification>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetNotificationActive(int id, SetBooleanRequest request, CancellationToken cancellationToken=default)
        => FromResult(await notificationService.ChangeNotificationStatusAsync(id, request, cancellationToken));
    
    [HttpDelete("{id:int}")]
    [ProducesResponseType<Notification>(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteNotification(int id, CancellationToken cancellationToken=default)
        => FromResult(await notificationService.DeleteNotificationAsync(id, cancellationToken));
}