using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using signalR.DTO.Request;
using signalR.Models.Local;
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
        public NotificationController(INotificationsService notificationsService) {

            _notificationsService = notificationsService;

        }

        [HttpPost("GetNotitifications")]
        public async Task<IActionResult> GetNotitifications([FromBody] NotificationRequest notificationRequest) {
            return Ok( await  _notificationsService.GetNotifications(notificationRequest.login));
        }

    }
}
