using Microsoft.AspNetCore.Mvc;
using NotificationsPush.DTO.Request;
using NotificationsPush.DTO.Response;
using NotificationsPush.Models.Local;
using NotificationsPush.Services.Interfaces;

namespace NotificationsPush.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly IAuthenticationService _uthenticationService;
        public AuthorizationController(IAuthenticationService uthenticationService)
        {
            _uthenticationService = uthenticationService;
        }

        [HttpPost("Authentication")]
        public async Task<IActionResult> Authentication([FromBody] UserRequest authentication)
        {
            // se transforma el obejeto que esta llegando al modelo local            
            User user = new User();
            user.password = authentication.password;
            user.user = authentication.user;
            user.type = authentication.type;

            UserAuthenticated userAuthenticated = await _uthenticationService.Authentication(user);

            GenericResponse<object> genericResponse =
                new GenericResponse<object>(
                    userAuthenticated.status,
                    userAuthenticated.message,
                    new
                    {
                        userAuthenticated.token,
                        userAuthenticated.minutesExpiresToken
                    }
                    );

            return Ok(genericResponse);

        }
    }
}
