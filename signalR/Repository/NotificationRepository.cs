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
using System.Data.SqlClient;

namespace signalR.Repository
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly IConfiguration _configuration;
        public NotificationRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<Notification>> GetNotifications(string serialTerminal)
        {
            List<Notification> list = new List<Notification>();

            using (var connecion = new SqlConnection(_configuration["ConnectionStrings:SQLServer"]))
            {
                connecion.Open();
                using (var command = new SqlCommand("sp_get_pending_notifications_terminal", connecion))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("in_terminal_serial", serialTerminal);
                    command.Parameters.Add(new SqlParameter("status", SqlDbType.Bit) { Direction = ParameterDirection.Output });
                    command.Parameters.Add(new SqlParameter("message", SqlDbType.VarChar, 100) { Direction = ParameterDirection.Output });

                    bool status;
                    string message;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            list.Add(new Notification()
                            {
                                notification_id = reader.GetInt64(0),
                                icon = reader.GetString(1),
                                picture = reader.GetString(2),
                                title = reader.GetString(3),
                                description = reader.GetString(4),

                            });
                        }
                    }
                    status = (bool)command.Parameters["status"].Value;
                    message = (string)command.Parameters["message"].Value;

                    if (!status)
                    {
                        return list;
                    }
                }
                return list;
            }
        }

        public async Task<SPDeleteNotification> DeleteNotification(int id, string login)
        {

            using (var connecion = new NpgsqlConnection(_configuration["ConnectionStrings:Postgres"]))
            {
                SPDeleteNotification sPDeleteNotification = new SPDeleteNotification();

                //connecion.Open();
                //using (var command = new NpgsqlCommand("delete_notifications", connecion))
                //{
                //    command.CommandType = System.Data.CommandType.StoredProcedure;

                //    command.Parameters.AddWithValue("in_login", login);
                //    command.Parameters.AddWithValue("in_notification_id", id);
                //    command.Parameters.Add(new NpgsqlParameter("status", NpgsqlDbType.Boolean) { Direction = ParameterDirection.Output });
                //    command.Parameters.Add(new NpgsqlParameter("message", NpgsqlDbType.Text) { Direction = ParameterDirection.Output });

                //    await command.ExecuteNonQueryAsync();

                //    sPDeleteNotification.status = (bool)command.Parameters["status"].Value;
                //    sPDeleteNotification.message = (string)command.Parameters["message"].Value;
                //}
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


        public bool UpdateSatusSentNotificationsTerminals(DataTable listNotificationsId, DataTable listTerminalsSerials, out string message )
        {
            bool status;
            message = "";

            using (var connecion = new SqlConnection(_configuration["ConnectionStrings:SQLServer"]))
            {
                connecion.Open();
                using (var command = new SqlCommand("sp_update_status_sent_notifications_terminals", connecion))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("list_serials_terminal", listTerminalsSerials);
                    command.Parameters.AddWithValue("list_notifications_id", listNotificationsId);
                    command.Parameters.Add(new SqlParameter("status", SqlDbType.Bit) { Direction = ParameterDirection.Output });
                    command.Parameters.Add(new SqlParameter("message", SqlDbType.VarChar, 100) { Direction = ParameterDirection.Output });

                    command.ExecuteNonQuery();

                    status = (bool)command.Parameters["status"].Value;
                    message = (string)command.Parameters["message"].Value;
                  
                }
              return status;
            }
        }
    }
}
