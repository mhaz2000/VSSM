namespace VSSM.Application.DTOs.BaseEntities.Reports
{
    public record AvailableQuantitySummaryWithDosePerVialDto : AvailableQuantitySummaryDto
    {
        public int DoesPerVial { get; set; }
    }
}
