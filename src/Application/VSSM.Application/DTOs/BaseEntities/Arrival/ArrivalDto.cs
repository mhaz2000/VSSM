using VSSM.Application.Helpers;

namespace VSSM.Application.DTOs.BaseEntities.Arrival;

public class ArrivalListDto
{
    public int ArrivalId { get; set; }
    public string ReportNumber { get; set; }
    public DateTime? Date { get; set; }
    public string FiscalPeriodName { get; set; }
    public bool? IsConfirmed { get; set; }
    public bool? IsDrafted { get; set; }
    public bool? IsCancelled { get; set; }
    public string DateText => Date is null ? string.Empty : Date.Value.ToShamsi();
    public string StatusText =>
        IsCancelled == true ? "حذف شده" :
        IsConfirmed == true ? "تایید شده" :
        IsDrafted == true ? "پیش‌نویس" : "نامشخص";
}






public class ArrivalGetDto
{
    public int ArrivalId { get; set; }
    public DateTime? Date { get; set; }
    public string ReportNumber { get; set; } = "";

    public int? FiscalPeriodId { get; set; }

    public DateTime? EntryDate { get; set; }

    public bool? IsConfirmed { get; set; }
    public bool? IsCancelled { get; set; }
    public bool? IsDrafted { get; set; }

    public string Note { get; set; } = "";
    public List<ArrivalLineDto> Lines { get; set; } = new();
}

