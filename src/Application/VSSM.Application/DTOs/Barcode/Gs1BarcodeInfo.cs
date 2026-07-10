namespace VSSM.Application.DTOs.Barcode;

public class Gs1BarcodeInfo
{
    public string? Gtin { get; set; }

    public string? Uid { get; set; }

    public string? BatchNumber { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public bool IsGs1 { get; set; }

}
