using System.ComponentModel.DataAnnotations;

namespace VSSM.Application.DTOs.BaseEntities.Reports;


public record AvailableQuantityQuantitySummaryMetaDto
{
    [Display(Name = "نوع کالا")]
    public string ItemClass { get; set; }

    [Display(Name = "Item Code")]
    public string ItemCode { get; set; }

    [Display(Name = "کالا")]
    public string ItemName { get; set; }

    [Display(Name = "جمع تعداد")]
    public long TotalCount { get; set; }
}
