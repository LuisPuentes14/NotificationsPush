using Microsoft.AspNetCore.Mvc.Formatters;
using midelware.Singleton.Logger;
using Newtonsoft.Json;
using Npgsql;
using Npgsql.Replication;
using NpgsqlTypes;
using signalR.Models.Local;
using signalR.Repository.Implementation;
using System.Data;

namespace signalR.Repository
{
    public class NotificationsRepository : INotificationsRepository
    {
        private readonly IConfiguration _configuration;
        public NotificationsRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<Notification>> GetNotifications(string clientLogin)
        {

            List<Notification> list = new List<Notification>();

            using (var connecion = new NpgsqlConnection(_configuration["ConnectionStrings:Postgres"]))
            {

                try
                {
                    connecion.Open();
                    using (var command = new NpgsqlCommand("get_notifications", connecion))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("in_login", clientLogin);
                        command.Parameters.Add(new NpgsqlParameter("status", NpgsqlDbType.Boolean) { Direction = ParameterDirection.Output });
                        command.Parameters.Add(new NpgsqlParameter("notification", NpgsqlDbType.Text) { Direction = ParameterDirection.Output });
                        command.Parameters.Add(new NpgsqlParameter("message", NpgsqlDbType.Text) { Direction = ParameterDirection.Output });

                        await command.ExecuteNonQueryAsync();

                        var status = (bool)command.Parameters["status"].Value;
                        string notifications = (string)command.Parameters["notification"].Value;
                        string message = (string)command.Parameters["message"].Value;

                        if (!status)
                        {
                            Console.WriteLine(message);
                            return list;
                        }

                        list = JsonConvert.DeserializeObject<List<Notification>>(notifications);
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.Message);
                    AppLogger.GetInstance().Info($"Error get_notifications_push_client: {e.Message}");
                }

                return list;

            }

        }
    }
}
