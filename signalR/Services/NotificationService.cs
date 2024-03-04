using signalR.Models.Local;
using signalR.Models.StoredProcedures;
using signalR.Repository.Implementation;
using signalR.Services.Interfaces;

namespace signalR.Services
{
    public class NotificationService: INotificationsService
    {
        private readonly INotificationRepository _notificationsRespository;
        public NotificationService(INotificationRepository notificationsRespository)
        {
            _notificationsRespository = notificationsRespository;
        }

        public async Task<List<Notification>> GetNotifications(string login)
        {
           return  await _notificationsRespository.GetNotifications(login);
        }


        public async Task<SPDeleteNotification> DeleteNotitification(string login, int notification_id)
        {
            return await _notificationsRespository.DeleteNotification(notification_id, login);
        }


    }
}
