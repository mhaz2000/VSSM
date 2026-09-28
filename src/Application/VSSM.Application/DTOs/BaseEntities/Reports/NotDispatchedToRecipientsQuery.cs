namespace VSSM.Application.DTOs.BaseEntities.Reports;

public record NotDispatchedToRecipientsQuery
{
    public long? ItemClassId { get; set; }
    public long? ItemId { get; set; }
    public long? FiscalPeriodId { get; set; }
    public string? FromDate { get; set; }
    public string? ToDate { get; set; }
}