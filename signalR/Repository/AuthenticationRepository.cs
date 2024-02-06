using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using signalR.Models.Local;
using signalR.Models.StoredProcedures;
using signalR.Repository.Interfaces;
using System.Data;

namespace signalR.Repository
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


            try
            {

                using (var connecion = new NpgsqlConnection(_configuration["ConnectionStrings:Postgres"]))
                {
                    connecion.Open();
                    using (var command = new NpgsqlCommand("validate_authentication_user", connecion))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("in_user_login", user.login);
                        command.Parameters.AddWithValue("in_password", user.password);

                        command.Parameters.Add(new NpgsqlParameter("out_status", NpgsqlDbType.Boolean) { Direction = ParameterDirection.Output });
                        command.Parameters.Add(new NpgsqlParameter("out_message", NpgsqlDbType.Text) { Direction = ParameterDirection.Output });
                        command.Parameters.Add(new NpgsqlParameter("out_email", NpgsqlDbType.Text) { Direction = ParameterDirection.Output });
                        command.Parameters.Add(new NpgsqlParameter("out_name", NpgsqlDbType.Text) { Direction = ParameterDirection.Output });

                        await command.ExecuteNonQueryAsync();

                        validateAuthenticationUser.status = (bool) command.Parameters["out_status"].Value ;
                        validateAuthenticationUser.message = command.Parameters["out_message"].Value.ToString();
                        validateAuthenticationUser.email = command.Parameters["out_email"].Value.ToString();                       
                        validateAuthenticationUser.name = command.Parameters["out_name"].Value.ToString();

                    }

                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                throw new Exception($"Error: {e.Message}");
            }

            return validateAuthenticationUser;
        }

    }
}