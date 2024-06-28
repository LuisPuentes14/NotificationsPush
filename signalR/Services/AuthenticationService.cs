using signalR.Services.Interfaces;
using signalR.Models.Local;
using signalR.Models.StoredProcedures;
using signalR.Repository.Interfaces;
using signalR.Utils.JWT;
using signalR.Utils.Encrypt;

namespace signalR.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IAuthenticationRepository _authenticationRepository;
        private readonly IJWT _JWT;
        private readonly IConfiguration _configuration;

        public AuthenticationService(IAuthenticationRepository authenticationRepository,
            IJWT JWT,
            IConfiguration configuration)
        {
            _authenticationRepository = authenticationRepository;
            _JWT = JWT;
            _configuration = configuration;
        }

        public async Task<UserAuthenticated> Authentication(User user)
        {

            user.password = DecryptPassword(user.password);

            UserAuthenticated userAuthenticated = new UserAuthenticated();

            if (user.type == "USER") {
                user.password = SHA256Encryption.EncryptWithSHA256(user.password);
            }           

            SPValidateAuthenticationUser validateAuthenticationUser = await _authenticationRepository.ValidateAuthenticationUser(user);

            if (validateAuthenticationUser.status)
            {
                userAuthenticated.token = _JWT.generateToken(user);
                userAuthenticated.minutesExpiresToken = _configuration["JwtSettings:TimeLifeMinutes"];
            }

            userAuthenticated.status = validateAuthenticationUser.status;
            userAuthenticated.message = validateAuthenticationUser.message;

            return userAuthenticated;
        }


        private string DecryptPassword(string password)
        {

            return AesDecryption.Decrypt(
                 Convert.FromBase64String(password),
                 Convert.FromBase64String(_configuration["AES:Key"]),
                 Convert.FromBase64String(_configuration["AES:IV"])
                 );
        }
    }
}
