using System.ComponentModel.DataAnnotations;

namespace VSSM.Application.DTOs.BaseEntities.Reports;

public record ArrivalByItemMetaDto
{
    [Display(Name = "کالا")]
    public string ItemName { get; init; }

    [Display(Name = "نوع کالا")]
    public string ItemClass { get; init; }

    [Display(Name = "جمع تعداد")]
    public long TotalQuantity { get; init; }
}