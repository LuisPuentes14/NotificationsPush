using System.ComponentModel.DataAnnotations;

namespace NotificationsPush.DTO.Request
{
    public class SendNotificationRequest
    {
        [Required]
        public List<string> terminal_serial { get; set; } = new List<string>();
        [Required]
        public long notification_id { get; set; }
        [Required]
        public string icon { get; set; }
        [Required]
        public string picture { get; set; }
        [Required]
        public string title { get; set; }
        [Required]
        public string description { get; set; }
    }
}
