namespace signalR.Models.Local
{
    public class SentTerminalsStatus
    {
        public List<string> terminalSend { get; set; } = new List<string>();
        public List<string> terminalNotSend { get; set; } = new List<string>();

    }
}
