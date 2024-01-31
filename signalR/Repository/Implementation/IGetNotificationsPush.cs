using signalR.Models;

namespace signalR.Repository.Implementation
{
    public interface IGetNotificationsPush
    {
        List<Notification> GetNotificationsPushClients();

    }
}
