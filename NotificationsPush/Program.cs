using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using NLog.Web;
using NotificationsPush.Hubs;
using NotificationsPush.Middlewares;
using NotificationsPush;
using NotificationsPush.HostedServices;
using NotificationsPush.IOC;
using Microsoft.AspNetCore.HttpsPolicy;

public partial class Program
{
    public async static Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Convierte la aplicacion en servicio de windows
        builder.Host.UseWindowsService();

        // Configura NLog
        //builder.Logging.ClearProviders();
        builder.Host.UseNLog();

        // Agrega servicios a la contenedor
        ConfigureServices(builder.Services, builder.Configuration);

        // Construir aplicacion
        var app = builder.Build();

        // Configura el pipeline de la aplicación
        ConfigurePipeline(app);

        app.Run();
    }

    static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        ConfigureAuthentication(services, configuration);
        services.AddSignalR();
        services.InyectarDependencia(configuration);
        services.AddHostedService<NotificationsHostedService>();
        ConfigureCors(services, configuration);
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // redirecciona la peticiones http a https al puerto configurado
        //services.Configure<HttpsRedirectionOptions>(options =>
        //{
        //    var url = configuration.GetSection("Kestrel:Endpoints:Https:Url").Get<string>();
        //    Uri uri = new Uri(url);
        //    options.HttpsPort = uri.Port;

        //});
    }

    static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var secretKey = Encoding.ASCII.GetBytes(jwtSettings["Secret"]);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });
    }


    static void ConfigureCors(IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .WithMethods("GET", "POST")
                    .AllowCredentials();
            });
        });
    }


    static void ConfigurePipeline(WebApplication app)
    {
        // Imprime el banner
        BannerApp.GenerateBanner();

        // Valida la conexión a la base de datos y otros parámetros
        var miServicio = app.Services.GetRequiredService<ValidateAppParameters>();
        miServicio.ValidateParameters();

        // Configura el pipeline HTTP
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseMiddleware<WebSocketsMiddleware>();
        app.UseMiddleware<LogMiddleware>();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseCors();
        app.MapControllers();
        app.MapHub<NotificationsHub>("/notificationsHub");
    }
}


