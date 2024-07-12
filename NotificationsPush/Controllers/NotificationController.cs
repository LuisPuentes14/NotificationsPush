using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationsPush.DTO.Request;
using NotificationsPush.DTO.Response;
using NotificationsPush.Models.Local;
using NotificationsPush.Services.Interfaces;

namespace NotificationsPush.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationsService _notificationsService;

        public NotificationController(INotificationsService notificationsService)
        {
            _notificationsService = notificationsService;
        }

        [HttpGet("GetNotitificationsPending/{serialTerminal}")]
        public async Task<IActionResult> GetNotitificationsPending(string serialTerminal)
        {

            List<NotificationPending> notifications = await _notificationsService.GetNotitificationsPending(serialTerminal);

            GenericResponse<NotificationPending> GenericResponse =
               new GenericResponse<NotificationPending>(
               true,
               "Proceso exitoso.",
               null,
               notifications);

            return Ok(GenericResponse);
        }


        [HttpPost("SendNotification")]
        public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequest sendNotificationRequest)
        {

            SendNotification sendNotification = new SendNotification();
            sendNotification.description = sendNotificationRequest.description;
            sendNotification.title = sendNotificationRequest.title;
            sendNotification.notification_id = sendNotificationRequest.notification_id;
            sendNotification.terminal_serial = sendNotificationRequest.terminal_serial;
            sendNotification.picture = sendNotificationRequest.picture;
            sendNotification.icon = sendNotificationRequest.icon;

            SentTerminalsStatus sentTerminalsStatus = await _notificationsService.SendNotification(sendNotification);

            GenericResponse<SentTerminalsStatus> GenericResponse =
                new GenericResponse<SentTerminalsStatus>(
                true,
                "Proceso exitoso.",
                sentTerminalsStatus);


            return Ok(GenericResponse);
        }



    }
}
