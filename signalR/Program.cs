using signalR.HostedServices;
using signalR.IOC;
using signalR.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.InyectarDependencia(builder.Configuration);
builder.Services.AddHostedService<NotificationsHostedService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            builder.WithOrigins("http://127.0.0.1:5500", "http://127.0.0.1:5501")
                .AllowAnyHeader()
                .WithMethods("GET", "POST")
                .AllowCredentials();
        });
});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors();

app.MapControllers();

app.MapHub<NotificationsHub>("/notificationsHub");

app.Run();
