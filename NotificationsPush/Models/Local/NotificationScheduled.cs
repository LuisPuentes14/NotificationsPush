namespace signalR.Models.Local
{
    public class NotificationScheduled
    {
        public Int64 notification_id { get; set; }
        public Int64 notification_schedule_id { get; set; }
        public string terminal_serial { get; set; }
        public string icon { get; set; }
        public string picture { get; set; }
        public string title { get; set; }
        public string description { get; set; }


    }
}
