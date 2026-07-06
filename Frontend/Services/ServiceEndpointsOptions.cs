namespace Frontend.Services;

public sealed class ServiceEndpointsOptions
{
    public const string SectionName = "ServiceEndpoints";

    public string EventService { get; set; } = "http://localhost:5076/";
    public string BookingService { get; set; } = "http://localhost:5045/";
    public string PreferenceService { get; set; } = "http://localhost:5176/";
    public string NotificationService { get; set; } = "http://localhost:5062/";
}
