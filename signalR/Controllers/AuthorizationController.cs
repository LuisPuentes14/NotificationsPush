using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace signalR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        [HttpGet]
        public IActionResult get() {


            return Ok("alejandro ");
        
        }
    }
}
