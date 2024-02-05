using signalR.Models;

namespace signalR.Repository.Implementation
{
    public interface IDeleteNotificationPushRepository
    {
        void DeleteNotificationsPushSent(int notificationId);

    }
}
