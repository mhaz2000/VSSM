using System.ComponentModel.DataAnnotations;

namespace VSSM.Presentation.Web.Models.Arrival;

public class ArrivalViewModel
{
    public int? ArrivalId { get; set; }

    [Display(Name = "تاریخ ورود")]
    [Required(ErrorMessage = "تاریخ ورود اجباری است.")]
    public string Date { get; set; } = string.Empty;

    [Display(Name = "شماره گزارش")]
    public string? ReportNumber { get; set; }

    [Display(Name = "دوره فعالیت")]
    public string? FiscalPeriodName { get; set; }

    public DateTime? EntryDate { get; set; }

    public bool IsConfirmed { get; set; }
    public bool IsCancelled { get; set; }
    public int? EditingLineId { get; set; }
    public bool IsDrafted { get; set; }

    [Display(Name = "ملاحظات")]
    public string? Note { get; set; }

    public List<ArrivalLineViewModel> Lines { get; set; } = new();
    public ArrivalLineViewModel NewLine { get; set; } = new();

    public string ArrivalType { get; set; } = "vaccine";

    public decimal? DosePerVial { get; set; }
    public decimal? NumVials { get; set; }
    public string? ProductionDateShamsi { get; set; }
    public string? ExpiryDateShamsi { get; set; }

}