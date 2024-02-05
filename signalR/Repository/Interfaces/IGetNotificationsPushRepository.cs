using signalR.Models.Local;

namespace signalR.Repository.Implementation
{
    public interface IGetNotificationsPushRepository
    {
       List<Notification> GetNotificationsPushClients(string clientLogin);

    }
}
