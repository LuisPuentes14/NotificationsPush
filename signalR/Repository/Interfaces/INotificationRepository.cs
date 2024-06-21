using signalR.Models.Local;
using signalR.Models.StoredProcedures;
using System.Data;

namespace signalR.Repository.Implementation
{
    public interface INotificationRepository
    {
        Task<List<Notification>> GetNotifications(string clientLogin);
        Task<SPDeleteNotification> DeleteNotification(int id, string login);
        void GenerateIncidenceExpirationNotifications();
        bool UpdateSatusSentNotificationsTerminals(DataTable listNotificationsId, DataTable listTerminalsSerials, out string message);

    }
}
