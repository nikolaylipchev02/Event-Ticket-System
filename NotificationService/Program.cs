using NotificationService.Application;
using NotificationService.Infrastructure;
using Microsoft.EntityFrameworkCore;

const string NOTIFICATION_SERVICE_DB_CONNECTION_STRING = "NotificationServiceDbConnection";

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
BindDependencies();
ConnectToPostgreSql();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
return;

void ConnectToPostgreSql() {
    string connectionString = builder.Configuration.GetConnectionString($"{NOTIFICATION_SERVICE_DB_CONNECTION_STRING}")
                              ?? throw new InvalidOperationException($"Connection string '{NOTIFICATION_SERVICE_DB_CONNECTION_STRING}' was not found");
    builder.Services.AddDbContext<NotificationServiceDbContext>(options => {
        options.UseNpgsql(connectionString);
    });
}

void BindDependencies() {
    builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
}