using BookingService.Application;
using BookingService.Infrastructure;
using Microsoft.EntityFrameworkCore;

const string BOOKING_SERVICE_DB_CONNECTION_STRING = "BookingServiceDbConnection";

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
    string connectionString = builder.Configuration.GetConnectionString($"{BOOKING_SERVICE_DB_CONNECTION_STRING}")
                              ?? throw new InvalidOperationException($"Connection string '{BOOKING_SERVICE_DB_CONNECTION_STRING}' was not found");
    builder.Services.AddDbContext<BookingServiceDbContext>(options => {
        options.UseNpgsql(connectionString);
    });
}

void BindDependencies() {
    builder.Services.AddScoped<IBookingRepository, BookingRepository>();
}