using VSSM.Application.Helpers;
namespace VSSM.Application.DTOs.BaseEntities.Dispatches;

public record DispatchPrintDto
{
    public string DispatchVoucherNumber { get; set; }
    public string InputPerson { get; set; }
    public string TransportMode { get; set; }
    public bool IsCopied { get; set; }
    public string Recipient { get; set; }
    public string RequestNumber { get; set; }
    public string DeliveryPlace { get; set; }
    public string FaDateText => Date?.ToShamsi() ?? string.Empty;
    public string DateText => Date?.ToShortDateString() ?? string.Empty;
    public DateTime? Date { get; set; }
    public string Note { get; set; }
    public IEnumerable<DispatchLinePrintDto> Items { get; set; }
    public int Id { get; internal set; }
    public bool? IsConfirmed { get; internal set; }
}

public record DispatchLinePrintDto
{
    public string Item { get; set; }
    public string BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? ExpiryDateText => ExpiryDate?.ToShortDateString() ?? string.Empty;
    public int? DoseCount { get; set; }
    public int? VialNumber { get; set; }
    public string? VialSize { get; set; }
    public string ColdStorageNumber { get; set; }
}