using System.ComponentModel.DataAnnotations;
using VSSM.Application.Helpers;

namespace VSSM.Application.DTOs.BaseEntities.Reports;

public record ArrivalByItemDetailDto
{
    // ── هدر ردیف ──
    [Display(Name = "تاریخ دریافت")]
    public DateTime? ReceiveDate { get; init; }
    public string ReceiveDateText => ReceiveDate?.ToShamsi() ?? "-";

    [Display(Name = "تاریخ ورود")]
    public DateTime? ArrivalDate { get; init; }
    public string ArrivalDateText => ArrivalDate?.ToShamsi() ?? "-";

    [Display(Name = "Entered by")]
    public string EnteredBy { get; init; }

    [Display(Name = "منبع")]
    public string Source { get; init; }

    [Display(Name = "شماره گزارش")]
    public string ReportNumber { get; init; }

    [Display(Name = "Bidding Number")]
    public string BiddingNumber { get; init; }

    [Display(Name = "Order number")]
    public int OrderNumber { get; init; }

    // ── جزئیات ──
    [Display(Name = "دوز-مقدار")]
    public long Quantity { get; init; }

    [Display(Name = "تعداد ویال")]
    public long NumVials { get; init; }

    [Display(Name = "دوز بر ویال")]
    public string DosePerVial { get; init; }

    [Display(Name = "سری ساخت")]
    public string BatchNumber { get; init; }

    [Display(Name = "سازنده")]
    public string Manufacturer { get; init; }

    [Display(Name = "مورد استفاده")]
    public string Use { get; init; }

    [Display(Name = "قیمت واحد")]
    public decimal UnitPrice { get; init; }

    [Display(Name = "مبلغ کل")]
    public decimal TotalPrice { get; init; }

    [Display(Name = "تاریخ انقضا")]
    public DateTime? ExpiryDate { get; init; }
    public string ExpiryDateText => ExpiryDate?.ToShamsi() ?? "-";

    [Display(Name = "تاریخ تولید")]
    public DateTime? ProductionDate { get; init; }
    public string ProductionDateText => ProductionDate?.ToShamsi() ?? "-";

    [Display(Name = "انبار")]
    public string Store { get; init; }

    [Display(Name = "نام تجاری")]
    public string CommercialName { get; init; }

    [Display(Name = "نوع VVM")]
    public string VvmType { get; init; }

    [Display(Name = "مرحله VVM")]
    public string VvmStage { get; init; }

    [Display(Name = "ملاحظات")]
    public string Note { get; init; }
}