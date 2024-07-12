using signalR.Models.Local;

namespace signalR.Utils.JWT
{
    public interface IJWT
    {
        string generateToken(User user);
    }
}
