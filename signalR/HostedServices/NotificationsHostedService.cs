using Microsoft.AspNetCore.SignalR;
using signalR.Models;
using signalR.Repository.Implementation;
using signalR.SignalR;
using System;
using System.Linq;
using System.Text.Json;

namespace signalR.HostedServices
{

    /// <summary>
    /// IHostedService: Control de tarea en segundo plano
    /// IDisposable: Liberar memoria
    /// </summary>
    public class NotificationsHostedService : IHostedService, IDisposable
    {

        private readonly IHubContext<NotificationsHub> _notificationsHub;
        private readonly IGenerateIncidenceExpirationNotifications _generateIncidenceExpirationNotifications;
        private readonly IGetNotificationsPush _getNotificationsPush;
        private readonly IConfiguration _configuration;
        private Timer _timer;

        public NotificationsHostedService(IHubContext<NotificationsHub> notificationsHub,
            IGenerateIncidenceExpirationNotifications generateIncidenceExpirationNotifications,
            IGetNotificationsPush getNotificationsPush,
            IConfiguration configuration)
        {
            _notificationsHub = notificationsHub;
            _generateIncidenceExpirationNotifications = generateIncidenceExpirationNotifications;
            _getNotificationsPush = getNotificationsPush;
            _configuration = configuration;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(SendNotifications, null, TimeSpan.Zero, TimeSpan.FromSeconds(int.Parse(_configuration["HostService:TimeFrameSeconds"])));

            return Task.CompletedTask;
        }

        private void SendNotifications(object state)
        {
            _generateIncidenceExpirationNotifications.SpGenerateIncidenceExpirationNotifications();

            //se obtienen las notificaiones que se van a enviar al usuario
            List<Notification> listNotifications = _getNotificationsPush.GetNotificationsPushClients();

            //se obtienen los clientes activos 
            List<ClientActive> listClientsActives = NotificationsHub.GetConnectedClient();

            //se obtiene las notificaciones de los usuarios que estan activos
            var notificationToSend = listClientsActives.Join(
                listNotifications,
                cliente => cliente.clientName,
                notification => notification.notification_send_push_login,
                (cliente, notification) => new
                {
                    clienteId = cliente.ConnectionId,
                    notifications = JsonSerializer.Serialize(notification)  
                }
                );

            // se envian las notificaciones a los usuarios
            foreach (var item in notificationToSend)
            {
                _notificationsHub.Clients.Client(item.clienteId).SendAsync(_configuration["HostService:MethodClient"], item.notifications);

                // queda pendiente mirar si es necesario crear una funcion para eliminar las notificaciones push
            }


        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }


    }
}
