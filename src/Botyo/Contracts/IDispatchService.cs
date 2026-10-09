using Botyo.Entities;

namespace Botyo.Contracts;

public interface IDispatchService
{
    Task DispatchNotificationAsync(Notification notification, CancellationToken cancellationToken=default);
}