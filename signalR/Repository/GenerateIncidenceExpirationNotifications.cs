using Microsoft.Extensions.FileProviders.Physical;
using Npgsql;
using signalR.Repository.Implementation;

namespace signalR.Repository
{
    public class GenerateIncidenceExpirationNotifications : IGenerateIncidenceExpirationNotifications
    {
        public void SpGenerateIncidenceExpirationNotifications() {

            var connectionString = "Host=localhost;Username=postgres;Password=12345;Database=polariscore";

            using (var connecion = new NpgsqlConnection(connectionString)) {

                try
                {
                    connecion.Open();
                    using (var command = new NpgsqlCommand("generate_incident_expiration_notification", connecion))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.ExecuteNonQuery();
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
