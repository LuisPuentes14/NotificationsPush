using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using signalR.Models;
using System.Collections.Concurrent;

namespace signalR.SignalR
{
    //[Authorize]
    public class NotificationsHub : Hub
    {


        //private static readonly ConcurrentDictionary<string, string> _users = new ConcurrentDictionary<string, string>();
        private static List<ClientActive> _users = new List<ClientActive>();

        public async Task SendMessage(string user, string message)
        {
            await Clients.Client(user).SendAsync(user, message);
            //await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public override async Task OnConnectedAsync()
        {
            var user = Context.GetHttpContext()?.Request.Query["user"];

            _users.Add(new ClientActive() { clientName = user, ConnectionId = Context.ConnectionId });
            //_users.TryAdd(user, Context.ConnectionId);           

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string user = Context.GetHttpContext()?.Request.Query["user"];
            ClientActive clients = _users.Where(x => x.clientName == user).FirstOrDefault();
            _users.Remove(clients);

            await base.OnDisconnectedAsync(exception);
        }

        public static List<ClientActive> GetConnectedClient() {
          return  _users;
        }

    }
}
