using System.Text.Json.Serialization;

namespace VSSM.Domain.ValueObjects;

public class PartialDate
{
    [JsonPropertyName("year")] public int? Year { get; set; }
    [JsonPropertyName("month")] public int? Month { get; set; }
    [JsonPropertyName("day")] public int? Day { get; set; }
}
