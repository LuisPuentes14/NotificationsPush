using NotificationsPush.Repository.Interfaces;
using NotificationsPush.Services;
using NotificationsPush.Repository;
using NotificationsPush.Utils.JWT;
using NotificationsPush.Services.Interfaces;
using NotificationsPush;

namespace NotificationsPush.IOC
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
