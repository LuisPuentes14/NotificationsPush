using signalR.Models.Local;
using signalR.Models.StoredProcedures;

namespace signalR.Repository.Interfaces
{
    public interface IAuthenticationRepository
    {
        Task<SPValidateAuthenticationUser> ValidateAuthenticationUser(User user);
    }
}
