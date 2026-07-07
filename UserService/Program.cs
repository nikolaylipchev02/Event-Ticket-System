using Microsoft.AspNetCore.Identity;
using UserService.Application;
using UserService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using UserService.Application.AuthService;
using UserService.Domain.Entities;

const string USER_SERVICE_DB_CONNECTION_STRING = "UserServiceDbConnection";

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
    string connectionString = builder.Configuration.GetConnectionString($"{USER_SERVICE_DB_CONNECTION_STRING}")
                              ?? throw new InvalidOperationException($"Connection string '{USER_SERVICE_DB_CONNECTION_STRING}' was not found");
    builder.Services.AddDbContext<UserServiceDbContext>(options => {
        options.UseNpgsql(connectionString);
    });
}

void BindDependencies() {
    builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
}
