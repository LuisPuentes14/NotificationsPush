namespace NotificationsPush.Models.StoredProcedures
{
    public class SPValidateAuthenticationUser
    {
        public bool status { get; set; }
        public string? message { get; set; }
        public string? email { get; set; }
        public string? name { get; set; }

    }
}
