using System.ComponentModel.DataAnnotations;

namespace signalR.DTO.Request
{
    public class AuthenticationRequest
    {
        [Required]
        public string? login { get; set; }

        [Required]
        public string? password { get; set; }
    }
}
