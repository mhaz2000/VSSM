using VSSM.Application.Helpers;

namespace VSSM.Application.DTOs.BaseEntities.Dispatches;

public record ToBeArrivedDispatchListDto
{
    public int Id { get; set; }
    public DateTime? Date { get; set; }
    public string DateText => Date is null ? "-" : Date.Value.ToShamsi();
    public string RequestNum { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public float? TotalVolume { get; set; }
    public DateTime? EntryDate { get; set; }
    public string EntryDateText => EntryDate is null ? "-" : EntryDate.Value.ToShamsi();

    public string DispatchVoucherNumber { get; internal set; }
}
