using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using Npgsql;
using Npgsql.Replication;
using NpgsqlTypes;
using signalR.Models.Local;
using signalR.Repository.Implementation;
using System.Data;

namespace signalR.Repository
{
    public class GetNotificationsPushRepository : IGetNotificationsPushRepository
    {
        private readonly IConfiguration _configuration;
        public GetNotificationsPushRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<Notification> GetNotificationsPushClients(string clientLogin)
        {

            using (var connecion = new NpgsqlConnection(_configuration["ConnectionStrings:Postgres"]))
            {

                try
                {
                    connecion.Open();
                    using (var command = new NpgsqlCommand("get_notifications_push_client", connecion))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("in_client_login", clientLogin);
                        command.Parameters.Add(new NpgsqlParameter("status", NpgsqlDbType.Boolean) { Direction = ParameterDirection.Output });
                        command.Parameters.Add(new NpgsqlParameter("notification", NpgsqlDbType.Text) { Direction = ParameterDirection.Output });
                        command.Parameters.Add(new NpgsqlParameter("message", NpgsqlDbType.Text) { Direction = ParameterDirection.Output });

                        command.ExecuteNonQuery();

                        bool status = (bool)command.Parameters["status"].Value;
                        string notifications = (string)command.Parameters["notification"].Value;
                        string message = (string)command.Parameters["message"].Value;

                        if (!status)
                        {
                            Console.WriteLine(message);
                            return null;
                        }

                        List<Notification> list = JsonConvert.DeserializeObject<List<Notification>>(notifications);
                        return list;

                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);
                    return null;
                }

            }

        }
    }
}
