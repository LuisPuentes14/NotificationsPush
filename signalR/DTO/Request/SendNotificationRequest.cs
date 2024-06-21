using System.ComponentModel.DataAnnotations;

namespace signalR.DTO.Request
{
    public class SendNotificationRequest
    {
        [Required]
        public List<string> terminal_serial { get; set; } = new List<string>();
        [Required]
        public Int64 notification_id { get; set; }
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
