namespace VSSM.Application.DTOs.BaseEntities.Dispatches;

public record ToBeArrivedDispatchListDto
{
    public int Id { get; set; }
    public DateTime? Date { get; set; }
    public string RequestNum { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string ReportNumber { get; set; }
    public float? TotalVolume { get; set; }
    public DateTime? EntryDate { get; set; }
}
