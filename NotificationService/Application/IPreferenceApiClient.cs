using SharedContracts;

namespace NotificationService.Application;

public interface IPreferenceApiClient {
    public Task<List<Guid>> GetMatchingUserIds(EventCity city, EventCategory category);
}