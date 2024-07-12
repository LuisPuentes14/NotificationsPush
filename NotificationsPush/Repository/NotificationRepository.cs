using NotificationsPush.Models.Local;
using NotificationsPush.Repository.Interfaces;
using NotificationsPush.Singleton.Logger;
using System.Data;
using System.Data.SqlClient;

namespace NotificationsPush.Repository
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly IConfiguration _configuration;
        public NotificationRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<NotificationPending>> GetNotitificationsPending(string serialTerminal)
        {
            List<NotificationPending> list = new List<NotificationPending>();

            using (var connecion = new SqlConnection(_configuration["ConnectionStrings:SQLServer"]))
            {
                connecion.Open();
                using (var command = new SqlCommand("sp_get_pending_notifications_terminal", connecion))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("in_terminal_serial", serialTerminal);
                    command.Parameters.Add(new SqlParameter("status", SqlDbType.Bit) { Direction = ParameterDirection.Output });
                    command.Parameters.Add(new SqlParameter("message", SqlDbType.VarChar, 100) { Direction = ParameterDirection.Output });

                    bool status;
                    string message;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            list.Add(new NotificationPending()
                            {
                                notification_pending_id = reader.GetInt64(0),
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

        public async void DeleteNotificationsPending(DataTable listNotificationsPending)
        {
            try
            {

                using (var connecion = new SqlConnection(_configuration["ConnectionStrings:SQLServer"]))
                {
                    connecion.Open();
                    using (var command = new SqlCommand("sp_delete_notifications_pending", connecion))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("list_notifications_pending", listNotificationsPending);
                        command.Parameters.Add(new SqlParameter("status", SqlDbType.Bit) { Direction = ParameterDirection.Output });
                        command.Parameters.Add(new SqlParameter("message", SqlDbType.VarChar, 100) { Direction = ParameterDirection.Output });

                        await command.ExecuteNonQueryAsync();

                        bool status = (bool)command.Parameters["status"].Value;
                        string message = (string)command.Parameters["message"].Value;

                        if (!status)
                        {
                            AppLogger.GetInstance().Error($"Error sp_delete_notifications_pending: {message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.GetInstance().Error($"Error sp_delete_notifications_pending: {ex.Message}");
            }


        }


        public void SavePendingTerminalNotifications(DataTable listNotificationsTerminalsSerialsSchedules)
        {

            try
            {
                using (var connecion = new SqlConnection(_configuration["ConnectionStrings:SQLServer"]))
                {
                    connecion.Open();
                    using (var command = new SqlCommand("sp_save_pending_terminal_notifications", connecion))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("list_notifications_serials_terminals_schedules", listNotificationsTerminalsSerialsSchedules);
                        command.Parameters.Add(new SqlParameter("status", SqlDbType.Bit) { Direction = ParameterDirection.Output });
                        command.Parameters.Add(new SqlParameter("message", SqlDbType.VarChar, 100) { Direction = ParameterDirection.Output });

                        command.ExecuteNonQuery();

                        bool status = (bool)command.Parameters["status"].Value;
                        string message = (string)command.Parameters["message"].Value;

                        if (!status)
                        {
                            AppLogger.GetInstance().Error($"Error sp_delete_notifications_pending: {message}");
                        }

                    }

                }
            }
            catch (Exception e)
            {
                AppLogger.GetInstance().Error($"Error sp_delete_notifications_pending: {e}");
            }


        }


        public async Task<List<NotificationScheduled>> GetScheduledNotifications()
        {
            List<NotificationScheduled> list = new List<NotificationScheduled>();

            using (var connecion = new SqlConnection(_configuration["ConnectionStrings:SQLServer"]))
            {
                try
                {
                    connecion.Open();
                    using (var command = new SqlCommand("sp_get_notifications_scheduled_shipping", connecion))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add(new SqlParameter("status", SqlDbType.Bit) { Direction = ParameterDirection.Output });
                        command.Parameters.Add(new SqlParameter("message", SqlDbType.VarChar, 100) { Direction = ParameterDirection.Output });


                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                list.Add(new NotificationScheduled()
                                {
                                    notification_id = reader.GetInt64(0),
                                    notification_schedule_id = reader.GetInt64(1),
                                    terminal_serial = reader.GetString(2),
                                    icon = reader.GetString(3),
                                    picture = reader.GetString(4),
                                    title = reader.GetString(5),
                                    description = reader.GetString(6),

                                });
                            }
                        }

                        bool status = (bool)command.Parameters["status"].Value;
                        string message = (string)command.Parameters["message"].Value;

                        if (!status)
                        {
                            AppLogger.GetInstance().Error($"Error sp_delete_notifications_pending: {message}");
                        }
                    }
                }
                catch (Exception e)
                {
                    AppLogger.GetInstance().Error($"Error generate_incident_expiration_notification: {e.Message}");
                }
            }

            return list;
        }
    }
}
