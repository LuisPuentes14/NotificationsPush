using signalR.DTO.Response;
using signalR.Models.Local;

namespace signalR.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResponse> Authentication(User user);
    }
}
