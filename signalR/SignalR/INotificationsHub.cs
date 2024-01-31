using signalR.Models;

namespace signalR.SignalR
{
    public interface INotificationsHub
    {
        List<ClientActive> GetConnectedClient();
    }
}
