using Confluent.Kafka;
using EventService.Application;
using EventService.Infrastructure;
using Microsoft.EntityFrameworkCore;

const string EVENT_SERVICE_DB_CONNECTION_STRING = "EventServiceDbConnection";
const string REDIS_INSTANCE_NAME = "EventService:";

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IProducer<string, string>>(sp => {
    IConfiguration configuration = sp.GetRequiredService<IConfiguration>();

    string bootstrapServers = configuration["Kafka:BootstrapServers"]
                              ?? throw new InvalidOperationException("Kafka bootstrap servers not configured");

    ProducerConfig producerConfig = new() {
            BootstrapServers = bootstrapServers,
            Acks = Acks.All,
            EnableIdempotence = true,
            LingerMs = 5
    };

    return new ProducerBuilder<string, string>(producerConfig).Build();
});

builder.Services.AddStackExchangeRedisCache(options => {
    options.Configuration = builder.Configuration.GetConnectionString("Redis")
                            ?? throw new InvalidOperationException("Redis connection string was not found");

    options.InstanceName = REDIS_INSTANCE_NAME;
});

builder.Services.AddHostedService<EventOutboxPublisherService>();

builder.Services.AddOpenApi();
builder.Services.AddControllers();

BindDependencies();
ConnectToPostgreSql();

WebApplication app = builder.Build();

app.MapControllers();

if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
return;

void ConnectToPostgreSql() {
    string connectionString = builder.Configuration.GetConnectionString($"{EVENT_SERVICE_DB_CONNECTION_STRING}")
                              ?? throw new InvalidOperationException(
                                      $"Connection string '{EVENT_SERVICE_DB_CONNECTION_STRING}' was not found");
    builder.Services.AddDbContext<EventServiceDbContext>(options => { options.UseNpgsql(connectionString); });
}

void BindDependencies() {
    builder.Services.AddScoped<IEventRepository, EventRepository>();
}