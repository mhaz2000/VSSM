using System.ComponentModel.DataAnnotations;

namespace VSSM.Application.DTOs.Reports;

public record ArrivalSummaryDto
{
    [Display(Name = "نوع کالا")]
    public string ItemClass { get; init; }

    [Display(Name = "کالا")]
    public string Item { get; init; }

    [Display(Name = "کد کالا")]
    public string Code { get; init; }

    [Display(Name = "جمع تعداد")]
    public long TotalQuantity { get; init; }

    [Display(Name = "جمع ویال")]
    public long TotalVials { get; init; }
}
