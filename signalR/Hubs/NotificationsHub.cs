using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using midelware.Singleton.Logger;
using signalR.Models.Local;
using signalR.Repository.Implementation;
using System;
using System.Collections.Concurrent;
using System.Text.Json;

namespace signalR.SignalR
{
    [Authorize]
    public class NotificationsHub : Hub
    {
        private readonly INotificationsRepository _getNotificationsPush;
        private readonly IConfiguration _configuration;
        private readonly IDeleteNotificationPushRepository _deleteNotificationPush;
        private static List<ClientActive> _users = new List<ClientActive>();

        public NotificationsHub(INotificationsRepository getNotificationsPush,
            IConfiguration configuration,
             IDeleteNotificationPushRepository deleteNotificationPush)
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

            AppLogger.GetInstance().Info($"Cliente conectado nombre :{clientActive.clientName}, id :{clientActive.ConnectionId} .");

            SendPendingNotifications(clientActive);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string user = Context.GetHttpContext()?.Request.Query["user"];

            ClientActive clients = _users.Where(x => x.clientName == user && x.ConnectionId == Context.ConnectionId).FirstOrDefault();
            _users.Remove(clients);

            AppLogger.GetInstance().Info($"Cliente desconectado nombre :{clients.clientName}, id :{clients.ConnectionId} .");

            await base.OnDisconnectedAsync(exception);
        }

        private async void SendPendingNotifications(ClientActive clientActive)
        {

            List<Notification> listNotifications =  await _getNotificationsPush.GetNotifications(clientActive.clientName);
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
