using System.ComponentModel.DataAnnotations;

namespace signalR.DTO.Response
{
    public class AuthenticationResponse
    {
        public bool status { get; set; }
        public string? message { get; set; }
        public string? token { get; set; }
        public string? minutesExpiresToken { get; set; }
    }
}
