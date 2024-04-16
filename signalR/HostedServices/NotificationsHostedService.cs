using Microsoft.AspNetCore.SignalR;
using midelware.Singleton.Logger;
using Newtonsoft.Json;
using Npgsql;
using signalR.Models.Local;
using signalR.Repository;
using signalR.Repository.Implementation;
using signalR.SignalR;
using System;
using System.Linq;
using System.Text;
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
        private readonly IConfiguration _configuration;
        private readonly INotificationRepository _notificationRepository;
        private Timer _timer;
        private CancellationTokenSource _cts;
        private Task _executingTask;

        public NotificationsHostedService(
            IHubContext<NotificationsHub> notificationsHub,             
            IConfiguration configuration,
            INotificationRepository notificationRepository)
        {
            _notificationsHub = notificationsHub;        
            _configuration = configuration;
            _notificationRepository = notificationRepository;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(GenerateIncidenceExpirationNotifications, null, TimeSpan.Zero, TimeSpan.FromSeconds(int.Parse(_configuration["HostService:TimeFrameGenerateNotificationSeconds"])));

            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _executingTask = Task.Run(() => ListenForNotifications(_cts.Token));

            return Task.CompletedTask;
        }

        private void GenerateIncidenceExpirationNotifications(object state)
        {
            // se ejecuta periodicamente para crear notificaciones de inicidencias que estan a punto de vencer
            _notificationRepository.GenerateIncidenceExpirationNotifications();
        }

        private async Task ListenForNotifications(CancellationToken cancellationToken)
        {
            try
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

                    //[0]= usuario a  quien va dirigida la notificacion   
                    //[1]= id de la notificacion que posterior mente se tiene que eliminar de la tabla de notificaciones push
                    //[2]= json que tiene informacion de la notificacion
                    string[] InformationNotificationSend = e.Payload.Split("*~*");

                    // busca las sesiones que tiene activas un usuario, que un usuario puedo estar conectado desde difentes dispositivos
                    List<ClientActive> listClientActives = listClientsActives.Where(c => c.clientName == InformationNotificationSend[0]).ToList();

                    foreach (var clientActive in listClientActives)
                    {
                        await _notificationsHub.Clients.Client(clientActive.ConnectionId).SendAsync(_configuration["Hub:MethodClient"], InformationNotificationSend[2]);
                    }

                };

                while (!cancellationToken.IsCancellationRequested)
                {
                    await connection.WaitAsync(cancellationToken);
                }

            }
            catch (Exception ex)
            {
                AppLogger.GetInstance().Error(ex.Message);               
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
