using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using signalR.DTO.Request;
using signalR.DTO.Response;
using signalR.Models.Local;
using signalR.Services.Interfaces;

namespace signalR.Controllers
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

        [HttpPost]
        public async Task< IActionResult> Authentication([FromBody] AuthenticationRequest authentication)
        {

            // se transforma el obejeto que esta llegando al modelo local 
            User user = new User();
            user.password = authentication.password;
            user.login = authentication.login;

            AuthenticationResponse authenticationResponse = await  _uthenticationService.Authentication(user);

            return Ok(authenticationResponse);

        }
    }
}
