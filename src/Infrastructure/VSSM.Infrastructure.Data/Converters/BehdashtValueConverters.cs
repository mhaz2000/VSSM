using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VSSM.Domain.ValueObjects;

namespace VSSM.Infrastructure.Data.Converters;

public class PartialDateConverter : ValueConverter<PartialDate?, string?>
{
    public PartialDateConverter()
        : base(
            v => v == null ? null : Format(v),
            s => string.IsNullOrWhiteSpace(s) ? null : Parse(s))
    {
    }

    private static string? Format(PartialDate v)
    {
        if (v.Year == null)
            return null;

        return $"{v.Year:0000}-{v.Month ?? 0:00}-{v.Day ?? 0:00}";
    }

    private static PartialDate? Parse(string s)
    {
        var parts = s.Split('-');
        if (parts.Length != 3)
            return null;

        return new PartialDate
        {
            Year = int.TryParse(parts[0], out var y) ? y : null,
            Month = int.TryParse(parts[1], out var m) ? m : null,
            Day = int.TryParse(parts[2], out var d) ? d : null
        };
    }
}

public class CodedValueConverter : ValueConverter<CodedValue?, string?>
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public CodedValueConverter()
        : base(
            v => v == null ? null : JsonSerializer.Serialize(v, Options),
            s => string.IsNullOrWhiteSpace(s) ? null : JsonSerializer.Deserialize<CodedValue>(s, Options))
    {
    }
}

public class OrgRefConverter : ValueConverter<OrgRef?, string?>
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public OrgRefConverter()
        : base(
            v => v == null ? null : JsonSerializer.Serialize(v, Options),
            s => string.IsNullOrWhiteSpace(s) ? null : JsonSerializer.Deserialize<OrgRef>(s, Options))
    {
    }
}
