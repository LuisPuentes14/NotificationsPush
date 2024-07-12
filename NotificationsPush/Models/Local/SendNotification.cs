namespace NotificationsPush.Models.Local
{
    public class SendNotification
    {
        public List<string> terminal_serial { get; set; } = new List<string>();
        public long notification_id { get; set; }
        public string icon { get; set; }
        public string picture { get; set; }
        public string title { get; set; }
        public string description { get; set; }
    }
}
