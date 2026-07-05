using PreferenceService.Application;
using PreferenceService.Infrastructure;
using Microsoft.EntityFrameworkCore;

const string PREFERENCE_SERVICE_DB_CONNECTION_STRING = "PreferenceServiceDbConnection";

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

BindDependencies();
ConnectToPostgreSql();

WebApplication app = builder.Build();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
return;

void ConnectToPostgreSql() {
    string connectionString = builder.Configuration.GetConnectionString($"{PREFERENCE_SERVICE_DB_CONNECTION_STRING}")
                              ?? throw new InvalidOperationException($"Connection string '{PREFERENCE_SERVICE_DB_CONNECTION_STRING}' was not found");
    builder.Services.AddDbContext<PreferenceServiceDbContext>(options => {
        options.UseNpgsql(connectionString);
    });
}

void BindDependencies() {
    builder.Services.AddScoped<IPreferenceRepository, PreferenceRepository>();
}