
using System.ComponentModel.DataAnnotations;

namespace VSSM.Application.DTOs.BaseEntities.Reports
{
    public record GroupedAvailableQuantityDetailDto
    {
        public AvailableQuantityDetailMetaDto Meta { get; set; }
        public List<AvailableQuantityDetailDto> Details { get; set; }

    }
    public record AvailableQuantityDetailMetaDto
    {
        [Display(Name = "نام کالا")]
        public string ItemName { get; internal set; }

        [Display(Name = "تعداد")]
        public long Count { get; set; }
    }
    public record AvailableQuantityDetailDto
    {
        public string ItemName { get; internal set; }
        public string BatchNumber { get; internal set; }
        public string ManufacturerName { get; internal set; }
        public DateTime? ExpiryDate { get; internal set; }
        public string ShamsiExpiryDate => ExpiryDate is not null ? ExpiryDate.Value.ToShortDateString() : string.Empty;
        public int DaysToBeExpired => ExpiryDate is not null ? (DateTime.Now - ExpiryDate.Value).Days : 0;
        public int DoesPerVial { get; internal set; }
        public string Store { get; internal set; }
        public string Recipient { get; internal set; }
        public DateTime? ProductionDate { get; internal set; }
        public string CommercialName { get; internal set; }
        public string Vvmstage { get; internal set; }
        public string Vvmtype { get; internal set; }
        public long? CurrentStock { get; internal set; }
    }
}
