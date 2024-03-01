using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Npgsql;
using signalR.Models.Local;
using signalR.Repository;
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
        private readonly IGenerateIncidenceExpirationNotificationsRepository _generateIncidenceExpirationNotifications;
        private readonly IConfiguration _configuration;
        private readonly IDeleteNotificationPushRepository _deleteNotificationPush;
        private Timer _timer;
        private CancellationTokenSource _cts;
        private Task _executingTask;

        public NotificationsHostedService(IHubContext<NotificationsHub> notificationsHub,
            IGenerateIncidenceExpirationNotificationsRepository generateIncidenceExpirationNotifications,
            INotificationsRepository getNotificationsPush,
            IConfiguration configuration,
            IDeleteNotificationPushRepository deleteNotificationPush )
        {
            _notificationsHub = notificationsHub;
            _generateIncidenceExpirationNotifications = generateIncidenceExpirationNotifications;
            _configuration = configuration;
            _deleteNotificationPush = deleteNotificationPush;
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

            using (var command = new NpgsqlCommand("LISTEN chanel_send_notification_push;", connection))
            {
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            connection.Notification += async (o, e) =>
            {
                Console.WriteLine($"Notificación recibida: {e.Payload}");        

                //se obtienen los clientes activos 
                List<ClientActive> listClientsActives = NotificationsHub.GetConnectedClient();          

                string[] InformationNotificationSend =  e.Payload.Split("*~*");        
                
                // busca las sesiones que tiene activas un usuario, que un usuario puedo estar conectado desde difentes dispositivos
                List <ClientActive> listClientActives = listClientsActives.Where(c => c.clientName == InformationNotificationSend[0]).ToList();

                if (listClientActives.Count != 0) {
                    foreach (var clientActive in listClientActives) {
                        await _notificationsHub.Clients.Client(clientActive.ConnectionId).SendAsync(_configuration["Hub:MethodClient"], InformationNotificationSend[2]);
                        _deleteNotificationPush.DeleteNotificationsPushSent(int.Parse(InformationNotificationSend[1]));
                    }              
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
