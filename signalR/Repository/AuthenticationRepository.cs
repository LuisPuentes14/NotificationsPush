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

        public async Task< SPValidateAuthenticationUser> ValidateAuthenticationUser(User user)
        {
            SPValidateAuthenticationUser validateAuthenticationUser = new SPValidateAuthenticationUser();

            using (var connecion = new NpgsqlConnection(_configuration["ConnectionStrings:Postgres"]))
            {

                try
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

                        validateAuthenticationUser.status = (bool)command.Parameters["out_status"].Value;
                        validateAuthenticationUser.message = (string)command.Parameters["out_message"].Value;
                        validateAuthenticationUser.email = (string)command.Parameters["out_email"].Value;
                        validateAuthenticationUser.name = (string)command.Parameters["out_name"].Value;

                    }

                    
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);
                }

                return validateAuthenticationUser;
            }
        }
    }
}