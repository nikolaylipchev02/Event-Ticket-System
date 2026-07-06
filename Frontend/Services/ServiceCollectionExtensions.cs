using Microsoft.Extensions.Options;

namespace Frontend.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFrontendBackendClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ServiceEndpointsOptions>(configuration.GetSection(ServiceEndpointsOptions.SectionName));

        services.AddHttpClient<IEventApiClient, EventApiClient>(ConfigureEventApiClient);
        services.AddHttpClient<IBookingApiClient, BookingApiClient>(ConfigureBookingApiClient);
        services.AddHttpClient<IPreferenceApiClient, PreferenceApiClient>(ConfigurePreferenceApiClient);
        services.AddHttpClient<INotificationApiClient, NotificationApiClient>(ConfigureNotificationApiClient);

        return services;
    }

    private static void ConfigureEventApiClient(IServiceProvider serviceProvider, HttpClient client)
    {
        client.BaseAddress = GetServiceBaseAddress(serviceProvider, options => options.EventService);
    }

    private static void ConfigureBookingApiClient(IServiceProvider serviceProvider, HttpClient client)
    {
        client.BaseAddress = GetServiceBaseAddress(serviceProvider, options => options.BookingService);
    }

    private static void ConfigurePreferenceApiClient(IServiceProvider serviceProvider, HttpClient client)
    {
        client.BaseAddress = GetServiceBaseAddress(serviceProvider, options => options.PreferenceService);
    }

    private static void ConfigureNotificationApiClient(IServiceProvider serviceProvider, HttpClient client)
    {
        client.BaseAddress = GetServiceBaseAddress(serviceProvider, options => options.NotificationService);
    }

    private static Uri GetServiceBaseAddress(IServiceProvider serviceProvider, Func<ServiceEndpointsOptions, string> selector)
    {
        ServiceEndpointsOptions options = serviceProvider.GetRequiredService<IOptions<ServiceEndpointsOptions>>().Value;
        string baseUrl = selector(options).Trim();

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException("A backend service base URL was not configured.");
        }

        return new Uri(baseUrl.EndsWith('/') ? baseUrl : $"{baseUrl}/", UriKind.Absolute);
    }
}
