using System.Diagnostics.Metrics;

namespace EventService.Infrastructure.Diagnostics;

public static class EventServiceMetrics {
    public const string METER_NAME = "EventService";

    static readonly Meter Meter = new(METER_NAME, "1.0");

    static readonly Counter<long> EventChanges = Meter.CreateCounter<long>("event_service_changes_total");
    static readonly Counter<long> EventWriteFailures = Meter.CreateCounter<long>("event_service_write_failures_total");
    static readonly Counter<long> CacheResults = Meter.CreateCounter<long>("event_service_cache_results_total");
    static readonly Counter<long> OutboxPublishes = Meter.CreateCounter<long>("event_service_outbox_publishes_total");

    static readonly Counter<long> OutboxPublishFailures =
            Meter.CreateCounter<long>("event_service_outbox_publish_failures_total");

    static readonly Histogram<double> EventWriteDurationMs =
            Meter.CreateHistogram<double>("event_service_event_write_duration_ms", "ms");

    static readonly Histogram<double> OutboxPublishDurationMs =
            Meter.CreateHistogram<double>("event_service_outbox_publish_duration_ms", "ms");

    public static void RecordEventChange(string operation, TimeSpan duration) {
        EventChanges.Add(1, new KeyValuePair<string, object?>("operation", operation));
        EventWriteDurationMs.Record(duration.TotalMilliseconds,
                new KeyValuePair<string, object?>("operation", operation));
    }

    public static void RecordEventWriteFailure(string operation, TimeSpan duration) {
        EventWriteFailures.Add(1, new KeyValuePair<string, object?>("operation", operation));
        EventWriteDurationMs.Record(duration.TotalMilliseconds,
                new KeyValuePair<string, object?>("operation", operation));
    }

    public static void RecordCacheResult(string operation, string result) {
        CacheResults.Add(
                1,
                new KeyValuePair<string, object?>("operation", operation),
                new KeyValuePair<string, object?>("result", result));
    }

    public static void RecordOutboxPublish(string result, TimeSpan duration) {
        if (result == "success") {
            OutboxPublishes.Add(1);
        } else {
            OutboxPublishFailures.Add(1);
        }

        OutboxPublishDurationMs.Record(duration.TotalMilliseconds,
                new KeyValuePair<string, object?>("result", result));
    }
}