using System.Globalization;
using EventService.Application.DTOs;

namespace EventService.Infrastructure.Caching;

public static class EventCacheKeys {
    public const string CacheVersionKey = "events:cache:version";

    public static string AllEvents(string version) {
        return $"events:v{version}:all";
    }

    public static string EventById(string version, Guid id) {
        return $"events:v{version}:event:{id:N}";
    }

    public static string Filtered(string version, FilterEventDto filterEventDto) {
        string signature = string.Join("|", new[] {
                $"city={filterEventDto.City?.ToString() ?? "*"}",
                $"category={filterEventDto.Category?.ToString() ?? "*"}",
                $"min={filterEventDto.MinPrice?.ToString(CultureInfo.InvariantCulture) ?? "*"}",
                $"max={filterEventDto.MaxPrice?.ToString(CultureInfo.InvariantCulture) ?? "*"}",
                $"from={filterEventDto.FromDate?.ToString("O", CultureInfo.InvariantCulture) ?? "*"}",
                $"to={filterEventDto.ToDate?.ToString("O", CultureInfo.InvariantCulture) ?? "*"}"
        });

        return $"events:v{version}:filter:{signature}";
    }
}