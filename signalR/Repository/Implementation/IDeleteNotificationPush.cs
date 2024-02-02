using signalR.Models;

namespace signalR.Repository.Implementation
{
    public interface IDeleteNotificationPush
    {
        void DeleteNotificationsPushSent(int notificationId);

    }
}
