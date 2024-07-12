using signalR.Models.Local;

namespace signalR.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task<UserAuthenticated> Authentication(User user);
    }
}
