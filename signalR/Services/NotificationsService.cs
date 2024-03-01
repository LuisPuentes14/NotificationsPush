using signalR.Models.Local;
using signalR.Repository.Implementation;
using signalR.Services.Interfaces;

namespace signalR.Services
{
    public class NotificationsService: INotificationsService
    {
        private readonly INotificationsRepository _notificationsRespository;
        public NotificationsService(INotificationsRepository notificationsRespository)
        {
            _notificationsRespository = notificationsRespository;

        }

        public async Task<List<Notification>> GetNotifications(string login)
        {
           return  await _notificationsRespository.GetNotifications(login);
        }


    }
}
