using Microsoft.AspNetCore.Mvc;
using signalR.DTO.Request;
using signalR.DTO.Response;
using signalR.Models.Local;
using signalR.Models.StoredProcedures;
using signalR.Services.Interfaces;

namespace signalR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationsService _notificationsService;

        public NotificationController(INotificationsService notificationsService)
        {
            _notificationsService = notificationsService;
        }
       
        [HttpGet("GetNotitifications/{serialTerminal}")]
        public async Task<IActionResult> GetNotitifications( string serialTerminal)
        {

            List<Notification> notifications = await _notificationsService.GetNotifications(serialTerminal);

            GenericResponse<Notification> GenericResponse =
               new GenericResponse<Notification>(
               true,
               "Proceso exitoso.",
               null,
               notifications);

            return Ok(GenericResponse);
        }

        [HttpPost("DeleteNotification")]
        public async Task<IActionResult> DeleteNotification([FromBody] DeleteNotitificationRequest deleteNotitification)
        {

            SPDeleteNotification sPDeleteNotification = await _notificationsService.DeleteNotitification(
                deleteNotitification.login, deleteNotitification.notification_id);

            DeleteNotitificationResponse deleteNotitificationResponse = new DeleteNotitificationResponse();

            deleteNotitificationResponse.status = sPDeleteNotification.status;
            deleteNotitificationResponse.message = sPDeleteNotification.message;


            return Ok(deleteNotitificationResponse);
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
