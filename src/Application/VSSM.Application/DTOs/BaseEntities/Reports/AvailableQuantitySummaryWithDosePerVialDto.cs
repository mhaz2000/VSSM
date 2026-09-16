namespace VSSM.Application.DTOs.BaseEntities.Reports;

public record AvailableQuantitySummaryWithDosePerVialDto : AvailableQuantitySummaryDto
{
    public string DoesPerVial { get; set; }
}