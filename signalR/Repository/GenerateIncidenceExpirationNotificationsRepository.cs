
using Microsoft.Extensions.FileProviders.Physical;
using Npgsql;
using signalR.Repository.Implementation;

namespace signalR.Repository
{
    public class GenerateIncidenceExpirationNotificationsRepository : IGenerateIncidenceExpirationNotificationsRepository
    {
        private readonly IConfiguration _configuration;
        public GenerateIncidenceExpirationNotificationsRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SpGenerateIncidenceExpirationNotifications()
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
                    Console.Error.WriteLine(e);
                }


            }


        }
    }
}
