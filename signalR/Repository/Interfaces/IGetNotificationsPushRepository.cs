using signalR.Models.Local;

namespace signalR.Repository.Implementation
{
    public interface INotificationsRepository
    {
      Task< List<Notification>> GetNotifications(string clientLogin);

    }
}
