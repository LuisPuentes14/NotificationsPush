using signalR.DTO.Request;
using signalR.Models.Local;
using signalR.Models.StoredProcedures;

namespace signalR.Services.Interfaces
{
    public interface INotificationsService
    {
        Task<List<Notification>> GetNotifications(string serialTerminal);
        Task<SPDeleteNotification> DeleteNotitification(string login, int notification_id);
        Task<SentTerminalsStatus> SendNotification(SendNotification sendNotificationRequest);
    }
}
