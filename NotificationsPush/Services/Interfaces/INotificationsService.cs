using NotificationsPush.Models.Local;

namespace NotificationsPush.Services.Interfaces
{
    public interface INotificationsService
    {
        Task<List<NotificationPending>> GetNotitificationsPending(string serialTerminal);
        Task<SentTerminalsStatus> SendNotification(SendNotification sendNotificationRequest);
    }
}
