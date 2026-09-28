namespace VSSM.Application.DTOs.BaseEntities.Reports;

public record IndicatorReportQuery
{
    public int? ItemClassId { get; set; }
    public int? ItemId { get; set; }
    public string? BatchNumber { get; set; }
}
