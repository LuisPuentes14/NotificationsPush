namespace NotificationsPush.Models.Local
{
    public class NotificationScheduled
    {
        public long notification_id { get; set; }
        public long notification_schedule_id { get; set; }
        public string terminal_serial { get; set; }
        public string icon { get; set; }
        public string picture { get; set; }
        public string title { get; set; }
        public string description { get; set; }


    }
}
