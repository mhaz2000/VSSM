using System.Text.Json.Serialization;

namespace VSSM.Domain.ValueObjects;

public class CodedValue
{
    [JsonPropertyName("value")] public string? Value { get; set; }
    [JsonPropertyName("coded_string")] public string? CodedString { get; set; }
    [JsonPropertyName("terminology_id")] public string? TerminologyId { get; set; }
}
