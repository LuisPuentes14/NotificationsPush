using signalR.SignalR;
using Microsoft.Extensions.Options;
using signalR.HostedServices;
using signalR.IOC;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using signalR.Middleware;
using midelware.Middlewares;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using System;
using NLog.Web;
using signalR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();


var Issuer = builder.Configuration["JwtSettings:Issuer"];
var Audience = builder.Configuration["JwtSettings:Audience"];
var SecretKey = Encoding.ASCII.GetBytes(builder.Configuration["JwtSettings:Secret"]);

builder.Services.AddAuthentication(d =>
{
    d.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    d.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(d =>
{

    d.RequireHttpsMetadata = false;
    d.SaveToken = false;
    d.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(SecretKey),
        ValidateIssuer = true,
        ValidIssuer = Issuer,
        ValidateAudience = true,
        ValidAudience = Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero

    };
});

builder.Services.AddSignalR();

builder.Services.InyectarDependencia(builder.Configuration);

builder.Services.AddHostedService<NotificationsHostedService>();

builder.Services.AddCors(options =>
{
    
   var AllowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

    options.AddDefaultPolicy(
        builder =>
        {
            builder.WithOrigins(AllowedOrigins)
                .AllowAnyHeader()
                .WithMethods("GET", "POST")
                .AllowCredentials();
        });
});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// redirecciona la peticiones http a https al puerto configurado
builder.Services.Configure<HttpsRedirectionOptions>(options =>
{
    var url = builder.Configuration.GetSection("Kestrel:Endpoints:Https:Url").Get<string>();
    Uri uri = new Uri(url);   
    options.HttpsPort = uri.Port;

});

//builder.Logging.ClearProviders();
builder.Host.UseNLog();

var app = builder.Build();

// imprime el banner 
BannerApp.GenerateBanner();

// valida la conexion a la base de datos y imprime el puerto con el que esta escuchando la app
var miServicio = app.Services.GetRequiredService<ValidateAppParameters>();
miServicio.ValidateParameters();

// Configure the HTTP request pipeline.
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

app.Run();
