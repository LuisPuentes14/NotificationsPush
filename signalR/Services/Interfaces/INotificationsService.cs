using signalR.Models.Local;
using signalR.Models.StoredProcedures;

namespace signalR.Services.Interfaces
{
    public interface INotificationsService
    {
        Task<List<Notification>> GetNotifications(string login);
        Task<SPDeleteNotification> DeleteNotitification(string login, int notification_id);
    }
}
