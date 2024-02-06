﻿using Microsoft.AspNetCore.Mvc.Formatters;
using midelware.Singleton.Logger;
using Newtonsoft.Json;
using Npgsql;
using Npgsql.Replication;
using NpgsqlTypes;
using signalR.Models;
using signalR.Repository.Implementation;
using System.Data;

namespace signalR.Repository
{
    public class DeleteNotificationPushRepository : IDeleteNotificationPushRepository
    {
        private readonly IConfiguration _configuration;
        public DeleteNotificationPushRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void DeleteNotificationsPushSent(int notificatioId)
        {
            using (var connecion = new NpgsqlConnection(_configuration["ConnectionStrings:Postgres"]))
            {

                try
                {
                    connecion.Open();
                    using (var command = new NpgsqlCommand("DELETE FROM notifications_send_push WHERE notification_id = @notification_id ", connecion))
                    {
                        command.CommandType = System.Data.CommandType.Text;

                        command.Parameters.AddWithValue("notification_id", notificatioId);

                        command.ExecuteNonQuery();

                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.Message);
                    AppLogger.GetInstance().Info($"Error eliminando notificacion push :{e.Message}.");
                }


            }
        }
    }
}
