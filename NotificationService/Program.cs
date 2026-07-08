using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NotificationService.Application;
using NotificationService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UserService.Application.Authentication;

const string NOTIFICATION_SERVICE_DB_CONNECTION_STRING = "NotificationServiceDbConnection";

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

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
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});

builder.Services.AddAuthorization();

builder.Services.AddOpenApi();
builder.Services.AddControllers();

BindDependencies();
ConnectToPostgreSql();

WebApplication app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

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