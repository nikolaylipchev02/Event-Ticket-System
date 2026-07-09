using System.Text;
using BookingService.Application;
using BookingService.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UserService.Application.Authentication;

const string BOOKING_SERVICE_DB_CONNECTION_STRING = "BookingServiceDbConnection";
const int JWT_CLOCK_SKEW_IN_MINUTES = 1;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddOpenApi();
builder.Services.AddControllers();

BindDependencies();
ConnectToPostgreSql();

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

void ConnectToPostgreSql() {
    string connectionString = builder.Configuration.GetConnectionString($"{BOOKING_SERVICE_DB_CONNECTION_STRING}")
                              ?? throw new InvalidOperationException(
                                      $"Connection string '{BOOKING_SERVICE_DB_CONNECTION_STRING}' was not found");
    builder.Services.AddDbContext<BookingServiceDbContext>(options => { options.UseNpgsql(connectionString); });
}

void BindDependencies() {
    builder.Services.AddScoped<IBookingRepository, BookingRepository>();
}