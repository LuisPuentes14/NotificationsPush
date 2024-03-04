using Microsoft.AspNetCore.Mvc.Formatters;
using midelware.Singleton.Logger;
using Newtonsoft.Json;
using Npgsql;
using Npgsql.Replication;
using NpgsqlTypes;
using signalR.Models.Local;
using signalR.Models.StoredProcedures;
using signalR.Repository.Implementation;
using System.Collections.Generic;
using System.Data;

namespace signalR.Repository
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly IConfiguration _configuration;
        public NotificationRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<Notification>> GetNotifications(string clientLogin)
        {

            List<Notification> list = new List<Notification>();

            using (var connecion = new NpgsqlConnection(_configuration["ConnectionStrings:Postgres"]))
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
                return list;
            }

        }

        public async Task<SPDeleteNotification> DeleteNotification(int id, string login)
        {

            using (var connecion = new NpgsqlConnection(_configuration["ConnectionStrings:Postgres"]))
            {

                SPDeleteNotification sPDeleteNotification = new SPDeleteNotification();


                connecion.Open();
                using (var command = new NpgsqlCommand("delete_notifications", connecion))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("in_login", login);
                    command.Parameters.AddWithValue("in_notification_id", id);
                    command.Parameters.Add(new NpgsqlParameter("status", NpgsqlDbType.Boolean) { Direction = ParameterDirection.Output });
                    command.Parameters.Add(new NpgsqlParameter("message", NpgsqlDbType.Text) { Direction = ParameterDirection.Output });

                    await command.ExecuteNonQueryAsync();

                    sPDeleteNotification.status = (bool)command.Parameters["status"].Value;
                    sPDeleteNotification.message = (string)command.Parameters["message"].Value;

                }

                return sPDeleteNotification;
            }
        }

        public void GenerateIncidenceExpirationNotifications()
        {

            using (var connecion = new NpgsqlConnection(_configuration["ConnectionStrings:Postgres"]))
            {

                try
                {
                    connecion.Open();
                    using (var command = new NpgsqlCommand("generate_incident_expiration_notification", connecion))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.Message);
                    AppLogger.GetInstance().Info($"Error generate_incident_expiration_notification: {e.Message}");
                }
            }
        }
    }
}
