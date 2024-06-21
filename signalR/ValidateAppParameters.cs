using Microsoft.Extensions.Options;
using midelware.Singleton.Logger;
using Npgsql;
using NpgsqlTypes;
using signalR.Models.Local;
using signalR.Models.StoredProcedures;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace signalR
{
    public class ValidateAppParameters
    {
        private readonly IConfiguration _configuration;
        public ValidateAppParameters( IConfiguration configuration )
        {
            _configuration = configuration;           
        }

        public void ValidateParameters()
        {
            VallidateConexionDataBase();
            ListenPort();
        }

        private void VallidateConexionDataBase()
        {
            try
            {
                using (var connecion = new SqlConnection(_configuration["ConnectionStrings:SQLServer"]))
                {
                    connecion.Open();
                }
                AppLogger.GetInstance().Info("Conexion establecida correctamente a la base de datos.");
            }
            catch (Exception ex)
            {
                AppLogger.GetInstance().Info("Error conectando a la base de datos.");
                Environment.Exit(1);
            }
        }

        public void ListenPort()
        {
            //var urlHttps = _configuration.GetSection("Kestrel:Endpoints:Https:Url").Get<string>();
            var urlHttp = _configuration.GetSection("Kestrel:Endpoints:Http:Url").Get<string>();

            //Uri uriHttps = new Uri(urlHttps);
            Uri uriHttp = new Uri(urlHttp);

            AppLogger.GetInstance().Info($"Escuchando en el puerto HTTP :{uriHttp.Port}.");
            //AppLogger.GetInstance().Info($"Escuchando en el puerto HTTPS :{uriHttps.Port}.");
        }


    }
}
