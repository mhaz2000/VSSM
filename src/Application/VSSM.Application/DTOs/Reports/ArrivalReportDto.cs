using System.ComponentModel.DataAnnotations;

namespace VSSM.Application.DTOs.Reports;

public record ArrivalReportDto
{
    [Display(Name = "نوع کالا")]
    public string ItemClass { get; init; }

    [Display(Name = "کالا")]
    public string Item { get; init; }

    [Display(Name = "کد کالا")]
    public string Code { get; init; }

    [Display(Name = "سازنده")]
    public string Manufacturer { get; init; }

    [Display(Name = "تاریخ ورود")]
    public string ArrivalDate { get; init; }

    [Display(Name = "شماره گزارش")]
    public string ReportNumber { get; init; }

    [Display(Name = "سری ساخت")]
    public string BatchNumber { get; init; }

    [Display(Name = "تعداد")]
    public long Quantity { get; init; }

    [Display(Name = "دوز بر ویال")]
    public string DosePerVial { get; init; }

    [Display(Name = "تعداد ویال")]
    public long NumVials { get; init; }

    [Display(Name = "تاریخ انقضا")]
    public string ExpiryDate { get; init; }

    [Display(Name = "انبار")]
    public string Store { get; init; }

    [Display(Name = "گیرنده")]
    public string Recipient { get; init; }
}
