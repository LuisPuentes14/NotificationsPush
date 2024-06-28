using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using midelware.Singleton.Logger;
using signalR.Models.Local;

namespace signalR.SignalR
{
    [Authorize]
    public class NotificationsHub : Hub
    {

        private static List<ClientActive> _users = new List<ClientActive>();

        public NotificationsHub() { }

        public async Task SendMessage(string user, string message)
        {
            await Clients.Client(user).SendAsync(user, message);
            //await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public override async Task OnConnectedAsync()
        {
            var user = Context.GetHttpContext()?.Request.Query["serial"];
            ClientActive clientActive = new ClientActive() { clientName = user, ConnectionId = Context.ConnectionId };
            _users.Add(clientActive);

            AppLogger.GetInstance().Info($"Cliente conectado nombre :{clientActive.clientName}, id :{clientActive.ConnectionId} .");
            AppLogger.GetInstance().Info($"Numero de clientes conectados :{_users.Count()}");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                string user = Context.GetHttpContext()?.Request.Query["serial"];

                ClientActive clients = _users.Where( x => x != null && x.clientName == user && x.ConnectionId == Context.ConnectionId).FirstOrDefault();

                if (clients is not null)
                {
                    _users.Remove(clients);
                    AppLogger.GetInstance().Info($"Cliente desconectado nombre :{clients.clientName}, id :{clients.ConnectionId} .");
                }

            }
            catch (Exception ex)
            {
                AppLogger.GetInstance().Info(ex.Message);
            }

            AppLogger.GetInstance().Info($"Numero de clientes conectados :{_users.Count()}");


            await base.OnDisconnectedAsync(exception);
        }

        public static List<ClientActive> GetConnectedClient()
        {
            return _users;
        }

    }
}
