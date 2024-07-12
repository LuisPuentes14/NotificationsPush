using NotificationsPush.Models.Local;
using NotificationsPush.Models.StoredProcedures;

namespace NotificationsPush.Repository.Interfaces
{
    public interface IAuthenticationRepository
    {
        Task<SPValidateAuthenticationUser> ValidateAuthenticationUser(User user);
    }
}
