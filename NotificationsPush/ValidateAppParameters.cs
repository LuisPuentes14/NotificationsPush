using midelware.Singleton.Logger;
using System;
using System.Data.SqlClient;


namespace signalR
{
    public class ValidateAppParameters
    {
        private readonly IConfiguration _configuration;
        public ValidateAppParameters(IConfiguration configuration)
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
                AppLogger.GetInstance().Info($"Error conectando a la base de datos:{ex}");
                Environment.Exit(1);
            }
        }

        public void ListenPort()
        {

            var urlHttps = _configuration.GetSection("Kestrel:Endpoints:Https:Url").Get<string>();
            var urlHttp = _configuration.GetSection("Kestrel:Endpoints:Http:Url").Get<string>();

            if (urlHttp is null && urlHttps is null)
            {
                AppLogger.GetInstance().Info($"No existen puertos de escucha para el servico");
                Environment.Exit(1);
            }

            if (urlHttp is not null)
            {
                Uri uriHttp = new Uri(urlHttp.Replace("*", "localhost"));
                AppLogger.GetInstance().Info($"Escuchando en el puerto HTTP :{uriHttp.Port}.");
            }

            if (urlHttps is not null)
            {
                Uri uriHttps = new Uri(urlHttps.Replace("*", "localhost"));
                AppLogger.GetInstance().Info($"Escuchando en el puerto HTTPS :{uriHttps.Port}.");
            }           

        }


    }
}
