using signalR.Models.Local;
using signalR.Models.StoredProcedures;

namespace signalR.Repository.Implementation
{
    public interface INotificationRepository
    {
        Task<List<Notification>> GetNotifications(string clientLogin);
        Task<SPDeleteNotification> DeleteNotification(int id, string login);
        void GenerateIncidenceExpirationNotifications();

    }
}
