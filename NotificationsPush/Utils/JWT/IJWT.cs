using NotificationsPush.Models.Local;

namespace NotificationsPush.Utils.JWT
{
    public interface IJWT
    {
        string generateToken(User user);
    }
}
