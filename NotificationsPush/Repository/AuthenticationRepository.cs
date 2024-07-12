using NotificationsPush.Models.Local;
using NotificationsPush.Models.StoredProcedures;
using NotificationsPush.Repository.Interfaces;
using System.Data;
using System.Data.SqlClient;

namespace NotificationsPush.Repository
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private readonly IConfiguration _configuration;
        public AuthenticationRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<SPValidateAuthenticationUser> ValidateAuthenticationUser(User user)
        {
            SPValidateAuthenticationUser validateAuthenticationUser = new SPValidateAuthenticationUser();

            using (var connecion = new SqlConnection(_configuration["ConnectionStrings:SQLServer"]))
            {
                connecion.Open();
                using (var command = new SqlCommand("sp_validate_user_notifications", connecion))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("in_name_user", user.user);
                    command.Parameters.AddWithValue("in_passwor_user", user.password);
                    command.Parameters.AddWithValue("in_type_user", user.type);

                    command.Parameters.Add(new SqlParameter("status", SqlDbType.Bit) { Direction = ParameterDirection.Output });
                    command.Parameters.Add(new SqlParameter("message", SqlDbType.VarChar, 500) { Direction = ParameterDirection.Output });

                    await command.ExecuteNonQueryAsync();

                    validateAuthenticationUser.status = (bool)command.Parameters["status"].Value;
                    validateAuthenticationUser.message = command.Parameters["message"].Value.ToString();

                }
            }

            return validateAuthenticationUser;
        }

    }
}