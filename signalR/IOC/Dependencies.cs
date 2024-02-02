using signalR.Repository;
using signalR.Repository.Implementation;
using signalR.Utils.JWT;

namespace signalR.IOC
{
    public static class Dependencies
    {
        private static IConfiguration _configuration;

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public static void InyectarDependencia(this IServiceCollection services, IConfiguration Configuration)    
        {

            // Segregación de interfaces           
            services.AddSingleton<IGenerateIncidenceExpirationNotifications, GenerateIncidenceExpirationNotifications>();
            services.AddSingleton<IGetNotificationsPush, GetNotificationsPush>(); 
            services.AddSingleton<IDeleteNotificationPush, DeleteNotificationPush>(); 
            services.AddSingleton<IJWT, JWT>();


        }
    }
}
