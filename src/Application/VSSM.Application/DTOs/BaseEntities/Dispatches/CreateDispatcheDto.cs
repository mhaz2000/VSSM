namespace VSSM.Application.DTOs.BaseEntities.Dispatches;

public class CreateDispatcheDto
{
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
    public int FiscalPeriodId { get; set; }
    public int TransportModeId { get; set; }
    public int ReceivingModeId { get; set; }
    public int EnteredById { get; set; }
    public float TotalVolume { get; set; }
    public string AirwaryWillReference { get; set; } = string.Empty;
    public string RequestNum { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public int ArrivalId { get; set; }
    public float TotalPrice { get; set; }
    public List<DispatchLineDto> DispatchLines { get; set; } = new();
}