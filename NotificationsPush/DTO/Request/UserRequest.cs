using System.ComponentModel.DataAnnotations;

namespace NotificationsPush.DTO.Request
{
    public class UserRequest
    {
        [Required]
        public string? user { get; set; }

        [Required]
        public string? password { get; set; }

        [Required]
        [RegularExpression("^(TERMINAL|USER)$", ErrorMessage = "The Type must be either 'TERMINAL' or 'USER'.")]
        public string? type { get; set; }
    }
}
