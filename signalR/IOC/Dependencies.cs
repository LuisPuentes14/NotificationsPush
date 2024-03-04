using signalR.Repository;
using signalR.Repository.Implementation;
using signalR.Repository.Interfaces;
using signalR.Utils.JWT;
using signalR.Services;
using signalR.Services.Interfaces;

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
            services.AddSingleton<ValidateAppParameters>();           
            services.AddSingleton<INotificationRepository, NotificationRepository>();           
            services.AddSingleton<IJWT, JWT>();
            services.AddScoped<IAuthenticationRepository, AuthenticationRepository>(); 
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<INotificationsService, NotificationService>();
        }
    }
}
