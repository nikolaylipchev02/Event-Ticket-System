using System.Text.Json;

namespace SharedContracts;

public static class SharedJsonOptions {
    public static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
}