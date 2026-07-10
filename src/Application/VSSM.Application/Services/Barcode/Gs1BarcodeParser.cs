using System.Globalization;
using VSSM.Application.DTOs.Barcode;

namespace VSSM.Application.Services.Barcode;

public class Gs1BarcodeParser : IGs1BarcodeParser
{
    public Gs1BarcodeInfo Parse(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            throw new ArgumentException("Barcode is empty");


        return new Gs1BarcodeInfo
        {
            Gtin = ExtractFixed(barcode, "01", 14),

            Uid = ExtractVariable(barcode, "21", 20),

            ExpiryDate = ParseExpiry(
                ExtractFixed(barcode, "17", 6)
            ),

            BatchNumber = ExtractVariable(barcode, "10", 0)
        };
    }

    private string? ExtractFixed(
        string barcode,
        string ai,
        int length)
    {
        var index = barcode.IndexOf(ai);

        if (index < 0)
            return null;


        var start = index + ai.Length;


        if (start + length > barcode.Length)
            return null;


        return barcode.Substring(start, length);
    }



    private string? ExtractVariable(
        string barcode,
        string ai,
        int maxLength)
    {
        var index = barcode.IndexOf(ai);

        if (index < 0)
            return null;

        var start = index + ai.Length;

        var value = barcode.Substring(start);

        var separatorIndex = value.IndexOf('\u001D');

        if (separatorIndex >= 0)
            value = value.Substring(0, separatorIndex);

        if (maxLength > 0 && value.Length > maxLength)
            value = value.Substring(0, maxLength);

        return value;
    }


    private DateTime? ParseExpiry(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return null;


        if (DateTime.TryParseExact(
            value,
            "yyMMdd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var date))
        {
            return date;
        }

        return null;
    }
}
