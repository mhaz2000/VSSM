using VSSM.Application.Helpers;

namespace VSSM.Application.DTOs.BaseEntities.Dispatches;


public record DispatchListDto
{
    public int Id { get; set; }
    public DateTime? Date { get; set; }
    public string DateText => Date is null ? string.Empty : Date.Value.ToShamsi();
    public string RequestNum { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public string DispatchVoucherNumber { get; set; }
    public float? TotalVolume { get; set; }
    public bool? IsDrafted { get; set; }
    public string IsDraftedText => IsDrafted.HasValue && IsDrafted.Value ? "بله" : "خیر";
    public bool? IsConfirmed { get; set; }
    public int ReceiverStatus { get; set; }
    public string ReceiverStatusText => ReceiverStatus == 0 ? "جدید" : "تایید";
}
