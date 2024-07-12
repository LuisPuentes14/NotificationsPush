using NotificationsPush.Models.Local;
using System.Data;

namespace NotificationsPush.Repository.Interfaces
{
    public interface INotificationRepository
    {
        Task<List<NotificationPending>> GetNotitificationsPending(string clientLogin);
        void DeleteNotificationsPending(DataTable listNotificationsPending);
        Task<List<NotificationScheduled>> GetScheduledNotifications();
        void SavePendingTerminalNotifications(DataTable listNotificationsPendingId);

    }
}
