namespace VSSM.Application.DTOs.BaseEntities.Reports;

public record AvailableQuantitySummaryDto
{
    public string ItemName { get; internal set; }
    public string ItemCode { get; internal set; }
    public long? CurrentStock { get; internal set; }
}