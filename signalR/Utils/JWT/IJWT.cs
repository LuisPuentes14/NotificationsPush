using signalR.Models;

namespace signalR.Utils.JWT
{
    public interface IJWT
    {
        string generateToken(User user);
    }
}
