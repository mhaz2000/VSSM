using System.ComponentModel.DataAnnotations;

namespace VSSM.Application.DTOs.BaseEntities.Reports
{
    public record GroupedExpiredItemDto
    {
        public ExpiredItemDtoDetailMetaDto Meta { get; set; }
        public List<ExpiredItemDto> Details { get; set; }

    }
    public record ExpiredItemDtoDetailMetaDto
    {
        [Display(Name = "نام کالا")]
        public string ItemName { get; internal set; }

        [Display(Name = "تعداد")]
        public long Count { get; set; }
    }
    public record ExpiredItemDto
    {
        public int VialNumber { get; set; }
        public long? CurrentStock { get; set; }
        public int DoesPerVial { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string ExpiryDateText => ExpiryDate is null ? string.Empty : ExpiryDate.Value.ToShortDateString();
        public string BatchNumber { get; set; }
        public string ManufacturerName { get; set; }
        public string Stock { get; set; }
        public string Vvmstage { get; set; }
        public string ItemName { get; internal set; }
    }
}
