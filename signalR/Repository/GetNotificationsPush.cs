using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using Npgsql;
using Npgsql.Replication;
using NpgsqlTypes;
using signalR.Models;
using signalR.Repository.Implementation;
using System.Data;

namespace signalR.Repository
{
    public class GetNotificationsPush : IGetNotificationsPush
    {

        public List<Notification> GetNotificationsPushClients() {           

            var connectionString = "Host=localhost;Username=postgres;Password=12345;Database=polariscore";

            using (var connecion = new NpgsqlConnection(connectionString))
            {

                try
                {
                    connecion.Open();
                    using (var command = new NpgsqlCommand("get_notifications_push", connecion))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
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
                catch (Exception)
                {

                    throw;
                }


            }


        }
    }
}
