namespace VSSM.Application.DTOs.BaseEntities.Dispatches;


public record DispatchListDto
{
    public int Id { get; set; }
    public DateTime? Date { get; set; }
    public string RequestNum { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public string ReportNumber { get; set; }
    public float? TotalVolume { get; set; }
    public bool? IsDrafted { get; set; }
    public bool? IsConfirmed { get; set; }
    public int ReceiverStatus { get; set; }
}
