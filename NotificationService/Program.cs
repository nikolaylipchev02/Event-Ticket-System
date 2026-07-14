using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NotificationService.Application;
using NotificationService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UserService.Application.Authentication;

const string NOTIFICATION_SERVICE_DB_CONNECTION_STRING = "NotificationServiceDbConnection";
const int JWT_CLOCK_SKEW_IN_MINUTES = 1;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

AddAuthentication();
AddHttpClients();
AddPersistence();
AddDependencies();
AddHostedServices();

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
                options.MapInboundClaims = false;

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

void AddHttpClients() {
    builder.Services.AddHttpClient<IPreferenceApiClient, PreferenceApiClient>(client => {
        client.BaseAddress = new Uri("http://localhost:5176");
    });
}

void AddPersistence() {
    string connectionString = builder.Configuration.GetConnectionString($"{NOTIFICATION_SERVICE_DB_CONNECTION_STRING}")
                              ?? throw new InvalidOperationException(
                                      $"Connection string '{NOTIFICATION_SERVICE_DB_CONNECTION_STRING}' was not found");

    builder.Services.AddDbContext<NotificationServiceDbContext>(options => { options.UseNpgsql(connectionString); });
}

void AddDependencies() {
    builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
}

void AddHostedServices() {
    builder.Services.AddHostedService<NotificationIntegrationEventConsumerService>();
}