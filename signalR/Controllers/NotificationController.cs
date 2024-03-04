using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using signalR.DTO.Request;
using signalR.DTO.Response;
using signalR.Models.Local;
using signalR.Models.StoredProcedures;
using signalR.Services.Interfaces;
using System.Diagnostics.Eventing.Reader;
using System.Security.AccessControl;

namespace signalR.Controllers
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

        [HttpPost("GetNotitifications")]
        public async Task<IActionResult> GetNotitifications([FromBody] NotificationRequest notificationRequest)
        {
            return Ok(await _notificationsService.GetNotifications(notificationRequest.login));
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

    }
}
