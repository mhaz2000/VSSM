namespace VSSM.Application.DTOs.BaseEntities.Reports
{
    public record AvailableQuantityQuantitySummaryDto : AvailableQuantitySummaryWithDosePerVialDto
    {
        public string ItemClass { get; set; }
        public string BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string ExpiryDateText => ExpiryDate is not null ? ExpiryDate.Value.ToShortDateString() : string.Empty;

    }
}
