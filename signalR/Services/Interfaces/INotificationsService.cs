using signalR.Models.Local;

namespace signalR.Services.Interfaces
{
    public interface INotificationsService
    {
        Task<List<Notification>> GetNotifications(string login);
    }
}
