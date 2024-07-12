using NotificationsPush.Models.Local;

namespace NotificationsPush.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task<UserAuthenticated> Authentication(User user);
    }
}
