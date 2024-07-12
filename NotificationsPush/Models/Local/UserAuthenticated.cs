using System.ComponentModel.DataAnnotations;

namespace NotificationsPush.Models.Local
{
    public class UserAuthenticated
    {
        public bool status { get; set; }
        public string? message { get; set; }
        public string? token { get; set; }
        public string? minutesExpiresToken { get; set; }
    }
}
