namespace VSSM.Application.DTOs.Reports;

public record EntryExitReportQuery
{
    // ── Search Filters ──
    public int? ItemClassId { get; set; }
    public string? ItemId { get; set; }
    public string? StoreId { get; set; }

    // ── General Filters ──
    public int? LowerReportId { get; set; }
    public int? ChainLevel { get; set; }
    public string? BatchNumber { get; set; }
    public int? ManufacturerId { get; set; }
    public int? VvmtypeId { get; set; }
    public int? VvmstageId { get; set; }
    public int? FiscalPeriodId { get; set; }
    public int? UserId { get; set; }

    // ── Date Filters ──
    public string? FromArrivalDate { get; set; }
    public string? ToArrivalDate { get; set; }
    public string? FromDispatchDate { get; set; }
    public string? ToDispatchDate { get; set; }

    // ── Report-Specific (Inbound) ──
    public int? CommercialNameId { get; set; }
    public string? ReportNumber { get; set; }
    public int? SourceId { get; set; }

    // ── Report-Specific (Outbound) ──
    public string? VoucherNumber { get; set; }
    public int? TransportationMethod { get; set; }
    public int? RecipientId { get; set; }
    public string? RequestNumber { get; set; }
    public string? BiddingNumber { get; set; }
}