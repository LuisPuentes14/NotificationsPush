using Microsoft.AspNetCore.SignalR;
using signalR.Models.Local;
using signalR.Repository.Implementation;
using signalR.Services.Interfaces;
using signalR.SignalR;
using System.Data;
using System.Text.Json;

namespace signalR.Services
{
    public class NotificationService : INotificationsService
    {
        private readonly INotificationRepository _notificationsRespository;
        private readonly IHubContext<NotificationsHub> _notificationsHub;
        private readonly IConfiguration _configuration;
        public NotificationService(INotificationRepository notificationsRespository,
             IHubContext<NotificationsHub> notificationsHub,
            IConfiguration configuration)
        {
            _notificationsRespository = notificationsRespository;
            _notificationsHub = notificationsHub;
            _configuration = configuration;
        }

        public async Task<List<NotificationPending>> GetNotitificationsPending(string serialTerminal)
        {
            List<NotificationPending> List = await _notificationsRespository.GetNotitificationsPending(serialTerminal);
            List<string> serials = new List<string>() { serialTerminal };

            _notificationsRespository.DeleteNotificationsPending(
                Utils.Utils.LongsToDataTable(List.Select(x => x.notification_pending_id).ToList()));

            return List;
        }


        public async Task<SentTerminalsStatus> SendNotification(SendNotification sendNotification)
        {

            List<ClientActive> listClientsActives = new List<ClientActive>();

            List<string> serialsTerminals = sendNotification.terminal_serial;
            listClientsActives = NotificationsHub.GetClientsConnected(serialsTerminals);           

            Notification notification = new Notification
            {
                notification_id = sendNotification.notification_id,
                title = sendNotification.title,
                description = sendNotification.description,
                picture = sendNotification.picture,
                icon = sendNotification.icon,
            };

            // obtiene un listado de terminales que estan conectados
            var resultado = from terminal in serialsTerminals
                            join client in listClientsActives
                            on terminal equals client.clientName
                            into deptGroup
                            from dept in deptGroup.DefaultIfEmpty()
                            select new
                            {
                                clientId = dept?.ConnectionId ?? "NO_CONECTADO",
                                serial_terminal = terminal
                            };


            //var tasks = new List<Task>();

            // Envia la notificacion a termiunales que estan conectados
            foreach (var item in resultado.Where(x => x.clientId != "NO_CONECTADO"))
            {
                // tasks.Add(
                _notificationsHub
                .Clients
                .Client(item.clientId)
                .SendAsync(
                    _configuration["Hub:MethodClient"],
                    JsonSerializer.Serialize(notification));
                    //);
            }

            // Esperar a que todas las tareas se completen
            //await Task.WhenAll(tasks);

            SentTerminalsStatus sentTerminalsStatus = new SentTerminalsStatus();

            // se obtiene el listdo de terminales que se les envio la notificación
            sentTerminalsStatus.terminalSend = resultado
                                                .Where(x => x.clientId != "NO_CONECTADO")
                                                .Select(x => x.serial_terminal)
                                                .ToList();

            // se obtiene el listados de terminales que no se les envio la notificacion
            sentTerminalsStatus.terminalNotSend = resultado
                                               .Where(x => x.clientId == "NO_CONECTADO")
                                               .Select(x => x.serial_terminal)
                                               .ToList();

            return sentTerminalsStatus;

        }



    }
}
