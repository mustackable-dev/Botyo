using Botyo.Entities;

namespace Botyo.Contracts;

public interface IDispatchService
{
    Task DispatchNotification(Notification notification);
}