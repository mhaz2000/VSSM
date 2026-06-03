using System.ComponentModel.DataAnnotations;

namespace VSSM.Application.DTOs.BaseEntities.Dispatches;
public class CreateDispatchDto
{
    public int SenderId { get; set; }
    [Required(ErrorMessage = "گیرنده اجباری است.")]
    public int ReceiverId { get; set; }
    public int FiscalPeriodId { get; set; }
    public string TransportMode { get; set; }
    public string ReceivingMode { get; set; }
    public int EnteredById { get; set; }
    public float TotalVolume { get; set; }
    public string AirwaryWillReference { get; set; } = string.Empty;
    public string RequestNum { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public int ArrivalId { get; set; }
    public float TotalPrice { get; set; }
    public List<DispatchLineDto> Items { get; set; } = new();
    public ulong DispatchVoucherNumber { get; set; }
    public string ReportNumber { get; set; }
}