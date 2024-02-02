using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using signalR.Models;
using signalR.Repository.Implementation;
using System.Collections.Concurrent;
using System.Text.Json;

namespace signalR.SignalR
{
    //[Authorize]
    public class NotificationsHub : Hub
    {
        private readonly IGetNotificationsPush _getNotificationsPush;
        private readonly IConfiguration _configuration;
        private readonly IDeleteNotificationPush _deleteNotificationPush;
        private static List<ClientActive> _users = new List<ClientActive>();

        public NotificationsHub(IGetNotificationsPush getNotificationsPush,
            IConfiguration configuration,
             IDeleteNotificationPush deleteNotificationPush)
        {
            _getNotificationsPush = getNotificationsPush;
            _configuration = configuration;
            _deleteNotificationPush = deleteNotificationPush;
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.Client(user).SendAsync(user, message);
            //await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public override async Task OnConnectedAsync()
        {
            var user = Context.GetHttpContext()?.Request.Query["user"];
            ClientActive clientActive = new ClientActive() { clientName = user, ConnectionId = Context.ConnectionId };
            _users.Add(clientActive);

            SendPendingNotifications(clientActive);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string user = Context.GetHttpContext()?.Request.Query["user"];
            ClientActive clients = _users.Where(x => x.clientName == user).FirstOrDefault();
            _users.Remove(clients);

            await base.OnDisconnectedAsync(exception);
        }

        private void SendPendingNotifications(ClientActive clientActive)
        {

            List<Notification> listNotifications = _getNotificationsPush.GetNotificationsPushClients(clientActive.clientName);
            foreach (Notification notification in listNotifications)
            {
                Clients.Client(clientActive.ConnectionId).SendAsync(_configuration["Hub:MethodClient"],
                  JsonSerializer.Serialize(notification));
                _deleteNotificationPush.DeleteNotificationsPushSent(notification.notification_id);
            }

        }

        public static List<ClientActive> GetConnectedClient()
        {
            return _users;
        }

    }
}
