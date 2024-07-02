using signalR.DTO.Request;
using signalR.Models.Local;

namespace signalR.Services.Interfaces
{
    public interface INotificationsService
    {
        Task<List<NotificationPending>> GetNotitificationsPending(string serialTerminal);       
        Task<SentTerminalsStatus> SendNotification(SendNotification sendNotificationRequest);
    }
}
