using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NotificationsPush.Models.Local;
using NotificationsPush.Singleton.Logger;
using System.Collections.Concurrent;

namespace NotificationsPush.Hubs
{
    [Authorize]
    public class NotificationsHub : Hub
    {
        private static ConcurrentDictionary<string, ClientActive> _users = new ConcurrentDictionary<string, ClientActive>();

        public NotificationsHub() { }

        public async Task SendMessage(string user, string message)
        {
            await Clients.Client(user).SendAsync(user, message);
            //await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                var user = Context.GetHttpContext()?.Request.Query["serial"];
                ClientActive clientActive = new ClientActive() { clientName = user, ConnectionId = Context.ConnectionId };
                _users.TryAdd(Context.ConnectionId, clientActive);

                AppLogger.GetInstance().Info($"Cliente conectado nombre :{clientActive.clientName}, id :{clientActive.ConnectionId} .");
                AppLogger.GetInstance().Info($"Numero de clientes conectados :{_users.Count()}");
            }
            catch (Exception ex)
            {

                AppLogger.GetInstance().Error(ex.Message);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                string user = Context.GetHttpContext()?.Request.Query["serial"];
                _users.TryRemove(Context.ConnectionId, out ClientActive clientActive);

                AppLogger.GetInstance().Info($"Cliente desconectado nombre :{user}, id :{Context.ConnectionId} .");
                AppLogger.GetInstance().Info($"Numero de clientes conectados :{_users.Count()}");
            }
            catch (Exception ex)
            {
                AppLogger.GetInstance().Error(ex.Message);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public static List<ClientActive> GetAllClients()
        {
            return _users.Select(x => x.Value).ToList();
        }

        public static List<ClientActive> GetClientsConnected(List<string> lisTerminalsSerials)
        {

            return _users
                .Select(x => x.Value)
                .Where(x => lisTerminalsSerials.Contains(x.clientName))
                .ToList();


        }

    }
}
