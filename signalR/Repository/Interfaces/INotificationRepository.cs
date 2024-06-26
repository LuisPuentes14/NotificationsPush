using signalR.Models.Local;
using signalR.Models.StoredProcedures;
using System.Data;

namespace signalR.Repository.Implementation
{
    public interface INotificationRepository
    {
        Task<List<NotificationPending>> GetNotifications(string clientLogin);
        void DeleteNotificationsPending(DataTable listNotificationsPending);
        Task<List<NotificationScheduled>> GetScheduledNotifications();
        void UpdateSatusSentNotificationsTerminals(DataTable listNotificationsPendingId );

    }
}
