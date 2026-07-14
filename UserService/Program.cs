using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using UserService.Application;
using UserService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UserService.Application.Authentication;
using UserService.Domain.Entities;

const string USER_SERVICE_DB_CONNECTION_STRING = "UserServiceDbConnection";
const int JWT_CLOCK_SKEW_IN_MINUTES = 1;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

AddAuthentication();
AddPersistence();
AddDependencies();

builder.Services.AddOpenApi();
builder.Services.AddControllers();

WebApplication app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
return;

void AddAuthentication() {
    builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

    builder.Services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options => {
                JwtOptions jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
                                        ?? throw new InvalidOperationException("JWT configuration was not found");

                options.TokenValidationParameters = new TokenValidationParameters {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtOptions.Issuer,
                        ValidAudience = jwtOptions.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                        ClockSkew = TimeSpan.FromMinutes(JWT_CLOCK_SKEW_IN_MINUTES)
                };
            });

    builder.Services.AddAuthorization();
}

void AddPersistence() {
    string connectionString = builder.Configuration.GetConnectionString($"{USER_SERVICE_DB_CONNECTION_STRING}")
                              ?? throw new InvalidOperationException(
                                      $"Connection string '{USER_SERVICE_DB_CONNECTION_STRING}' was not found");

    builder.Services.AddDbContext<UserServiceDbContext>(options => { options.UseNpgsql(connectionString); });
}

void AddDependencies() {
    builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

    builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
}