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
    
        private static List<ClientActive> _users = new List<ClientActive>();

        public NotificationsHub() {}

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

        public static List<ClientActive> GetConnectedClient()
        {
            return _users;
        }

    }
}
