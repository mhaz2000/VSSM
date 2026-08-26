using System.Text.Json.Serialization;

namespace VSSM.Domain.ValueObjects;

public class OrgRef
{
    [JsonPropertyName("issuer")] public string? Issuer { get; set; }
    [JsonPropertyName("assigner")] public string? Assigner { get; set; }
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("type")] public string? Type { get; set; }
}
