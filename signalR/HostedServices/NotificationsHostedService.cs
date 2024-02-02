using Microsoft.AspNetCore.SignalR;
using Npgsql;
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
        private CancellationTokenSource _cts;
        private Task _executingTask;

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
            _timer = new Timer(GenerateNotifications, null, TimeSpan.Zero, TimeSpan.FromSeconds(int.Parse(_configuration["HostService:TimeFrameGenerateNotificationSeconds"])));

            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _executingTask = Task.Run(() => ListenForNotifications(_cts.Token));

            return Task.CompletedTask;
        }

        private void GenerateNotifications(object state)
        {
              // se ejecuta periodicamente para crear notificaciones de inicidencias que estan a punto de vencer
            _generateIncidenceExpirationNotifications.SpGenerateIncidenceExpirationNotifications();         
        }

        private async Task ListenForNotifications(CancellationToken cancellationToken)
        {
            using var connection = new NpgsqlConnection(_configuration["ConnectionStrings:Postgres"]);
            await connection.OpenAsync(cancellationToken);

            using (var command = new NpgsqlCommand("LISTEN canal_cambios;", connection))
            {
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            connection.Notification += (o, e) =>
            {
                Console.WriteLine($"Notificación recibida: {e.Payload}");
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
                    _notificationsHub.Clients.Client(item.clienteId).SendAsync(_configuration["Hub:MethodClient"], item.notifications);

                    // queda pendiente mirar si es necesario crear una funcion para eliminar las notificaciones push
                }
            };

            while (!cancellationToken.IsCancellationRequested)
            {
                await connection.WaitAsync(cancellationToken);
            }
        }


        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            _cts?.Cancel();     

            // Espera a que la tarea termine o se detenga debido a la cancelación
            await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));

            cancellationToken.ThrowIfCancellationRequested();           
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }


    }
}
