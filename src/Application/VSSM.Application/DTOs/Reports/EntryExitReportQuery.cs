namespace VSSM.Application.DTOs.Reports;

public record EntryExitReportQuery
{
    public int? ItemClassId { get; set; }
    public string ItemId { get; set; }
    public string StoreId { get; set; }
    public int? RecipientId { get; set; }
    public int? SourceId { get; set; }
    public int? ManufacturerId { get; set; }
    public int? VvmstageId { get; set; }
    public int? FiscalPeriodId { get; set; }

    public string? BatchNumber { get; set; }
    public string? ReportNumber { get; set; }
    public string? VoucherNumber { get; set; }

    public string? FromArrivalDate { get; set; }
    public string? ToArrivalDate { get; set; }
    public string? FromDispatchDate { get; set; }
    public string? ToDispatchDate { get; set; }
}
