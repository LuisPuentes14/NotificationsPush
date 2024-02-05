using signalR.Services.Interfaces;
using signalR.DTO.Response;
using signalR.Models.Local;
using signalR.Models.StoredProcedures;
using signalR.Repository.Interfaces;
using signalR.Utils.JWT;

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

        public async Task<AuthenticationResponse> Authentication(User user)
        {

            AuthenticationResponse authenticationResponse = new AuthenticationResponse();

            user.password = SHA256Encryption.EncryptWithSHA256(user.password);          

            SPValidateAuthenticationUser validateAuthenticationUser = await _authenticationRepository.ValidateAuthenticationUser(user);

            if (validateAuthenticationUser.status)
            {
                user.email = validateAuthenticationUser.email;
                user.name = validateAuthenticationUser.name;

                authenticationResponse.token = _JWT.generateToken(user);
                authenticationResponse.minutesExpiresToken = _configuration["JwtSettings:TimeLifeMinutes"];
            }

            authenticationResponse.status = validateAuthenticationUser.status;
            authenticationResponse.message = validateAuthenticationUser.message;

            return authenticationResponse;
        }
    }
}
