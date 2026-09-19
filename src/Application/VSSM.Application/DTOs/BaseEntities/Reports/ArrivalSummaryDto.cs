using System.ComponentModel.DataAnnotations;
using VSSM.Application.Helpers;

namespace VSSM.Application.DTOs.Reports;


public record ArrivalSummaryDto
{
    [Display(Name = "نوع کالا")]
    public string ItemClass { get; init; }

    [Display(Name = "کالا")]
    public string Item { get; init; }

    [Display(Name = "کد کالا")]
    public string ItemCode { get; init; }

    [Display(Name = "سازنده")]
    public string Manufacturer { get; init; }

    [Display(Name = "منبع")]
    public string Source { get; init; }

    [Display(Name = "فعالیت")]
    public string Activity { get; init; }

    [Display(Name = "تاریخ ورود")]
    public DateTime? ArrivalDate { get; init; }
    public string ArrivalDateText => ArrivalDate?.ToShamsi() ?? "-";

    [Display(Name = "شماره گزارش")]
    public string ReportNumber { get; init; }

    [Display(Name = "شماره سفارش")]
    public int OrderNumber { get; init; }

    [Display(Name = "تعداد")]
    public long Quantity { get; init; }

    [Display(Name = "سری ساخت")]
    public string BatchNumber { get; init; }

    [Display(Name = "دوز بر ویال")]
    public string DosePerVial { get; init; }

    [Display(Name = "تعداد ویال")]
    public long NumVials { get; init; }

    [Display(Name = "قیمت واحد")]
    public decimal UnitPrice { get; init; }

    [Display(Name = "مبلغ کل")]
    public decimal TotalPrice { get; init; }

    [Display(Name = "تاریخ انقضا")]
    public DateTime? ExpiryDate { get; init; }
    public string ExpiryDateText => ExpiryDate?.ToShamsi() ?? "-";
}
