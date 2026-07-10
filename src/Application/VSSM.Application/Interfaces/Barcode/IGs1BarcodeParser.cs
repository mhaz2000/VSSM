using VSSM.Application.DTOs.Barcode;

namespace VSSM.Application.Services.Barcode;

public interface IGs1BarcodeParser
{
    Gs1BarcodeInfo Parse(string barcode);
}

