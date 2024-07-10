using Microsoft.AspNetCore.SignalR;
using midelware.Singleton.Logger;
using signalR.Models.Local;
using signalR.Repository.Implementation;
using signalR.SignalR;
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
            // Crea un Timer que no hace nada inicialmente y no tiene un periodo definido aún
            Timer timer = new Timer(GetScheduledNotifications, null, Timeout.Infinite, Timeout.Infinite);

            // Calcula cuánto tiempo falta para la próxima hora en punto
            DateTime now = DateTime.Now;
            DateTime nextHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddHours(int.Parse(_configuration["HostService:TimeFrameNotificationHours"]));
            TimeSpan delay = nextHour - now;

            // Configura el temporizador para que se active en la próxima hora en punto y luego cada hora
            timer.Change(delay, TimeSpan.FromHours(int.Parse(_configuration["HostService:TimeFrameNotificationHours"])));

            AppLogger.GetInstance().Info($"Timer configurado para iniciar a las: {nextHour} y se ejecutara cada {_configuration["HostService:TimeFrameNotificationHours"]} horas.");

            return Task.CompletedTask;
        }

        private async void GetScheduledNotifications(object state)
        {
            // se ejecuta periodicamente para obtener las notificaciones programadas
            List<NotificationScheduled> notificationScheduleds =
                await _notificationRepository.GetScheduledNotifications();

            // si  no hay notificaciones para enviar omite el proceso
            if (notificationScheduleds.Count() == 0)
            {
                AppLogger.GetInstance().Info($"No ahi notificaciones programadas para enviar.");
                return;
            }

            // obtiene los clientes activos
            List<ClientActive> listClientsActives = new List<ClientActive>();
            listClientsActives = NotificationsHub
                .GetClientsConnected(notificationScheduleds.Select(x => x.terminal_serial).ToList());

            // obtiene un listado de terminales que estan conectados
            var resultado = from terminal in notificationScheduleds
                            join client in listClientsActives
                            on terminal.terminal_serial equals client.clientName
                            into deptGroup
                            from dept in deptGroup.DefaultIfEmpty()
                            select new
                            {
                                clientId = dept?.ConnectionId ?? "NO_CONECTADO",
                                serial_terminal = terminal.terminal_serial,
                                notification_id = terminal.notification_id,
                                notification_schedule_id = terminal.notification_schedule_id,
                                icon = terminal.icon,
                                picture = terminal.picture,
                                title = terminal.title,
                                description = terminal.description,
                            };

            // Envia la notificacion a termiunales que estan conectados
            foreach (var item in resultado.Where(x => x.clientId != "NO_CONECTADO"))
            {
                _notificationsHub.Clients.Client(
                   item.clientId).SendAsync(_configuration["Hub:MethodClient"],
                   JsonSerializer.Serialize(new Notification
                   {
                       notification_id = item.notification_id,
                       icon = item.icon,
                       picture = item.picture,
                       title = item.title,
                       description = item.description,
                   })
                   );
            }

            var notificationSendTerminals = resultado
                .Where(x => x.clientId == "NO_CONECTADO")
                .Select(x =>
                      new
                      {
                          x.notification_id,
                          x.serial_terminal,
                          x.notification_schedule_id
                      }).ToList();

            _notificationRepository.SavePendingTerminalNotifications(Utils.Utils.ToDataTable(notificationSendTerminals));

            AppLogger.GetInstance().Info($"Notificiones programadas enviadas.");
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
