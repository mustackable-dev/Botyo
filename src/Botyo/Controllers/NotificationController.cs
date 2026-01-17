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
    public async Task<IActionResult> CreateNotification(NotificationDto notification)
    {
        Result<Notification> result = await notificationService.CreateNotification(notification);

        if (!workerService.IsRunning())
        {
            await workerService.StartAsync(CancellationToken.None);
        }
        
        return FromResult(result);
    }
    
    [HttpPost("{id}/Run")]
    [ProducesResponseType<Notification>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RunNotification(int id)
        => FromResult(await notificationService.RunNotificationManually(id));
    
    [HttpPut("{id}")]
    [ProducesResponseType<Notification>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateNotification(int id, NotificationUpdateDto notification)
        => FromResult(await notificationService.UpdateNotification(id, notification));

    [HttpGet]
    [ProducesResponseType<Notification>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllNotifications()
        => FromResult(await notificationService.GetAllNotifications());
    
    [HttpGet("{id}")]
    [ProducesResponseType<Notification>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNotification(int id)
        => FromResult(await notificationService.GetNotification(id));
    
    [HttpPatch("{id}/Start")]
    [ProducesResponseType<Notification>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartNotification(int id)
        => FromResult(await notificationService.ChangeNotificationStatus(id, true));
    
    [HttpPatch("{id}/Stop")]
    [ProducesResponseType<Notification>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StopNotification(int id)
        => FromResult(await notificationService.ChangeNotificationStatus(id, false));
    
    [HttpDelete("{id}")]
    [ProducesResponseType<Notification>(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteNotification(int id)
        => FromResult(await notificationService.DeleteNotification(id));
}