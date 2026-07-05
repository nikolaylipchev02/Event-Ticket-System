using EventService.Application;
using EventService.Infrastructure;
using Microsoft.EntityFrameworkCore;

const string EVENT_SERVICE_DB_CONNECTION_STRING = "EventServiceDbConnection";

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
    string connectionString = builder.Configuration.GetConnectionString($"{EVENT_SERVICE_DB_CONNECTION_STRING}")
                              ?? throw new InvalidOperationException($"Connection string '{EVENT_SERVICE_DB_CONNECTION_STRING}' was not found");
    builder.Services.AddDbContext<EventServiceDbContext>(options => {
        options.UseNpgsql(connectionString);
    });
}

void BindDependencies() {
    builder.Services.AddScoped<IEventsRepository, EventsRepository>();
}