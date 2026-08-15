namespace VSSM.Infrastructure.Data.Models;

public partial class TblArrival
{
    public int Id { get; set; }
    public DateTime? Date { get; set; }
    public string ReportNumber { get; set; } = null!;
    public int? FiscalPeriodId { get; set; }
    public DateTime? EntryDate { get; set; }
    public int? EnteredById { get; set; }
    public bool? IsConfirmed { get; set; }
    public bool? IsCancelled { get; set; }
    public bool? IsDrafted { get; set; }
    public DateTime? ConfirmationTime { get; set; }
    public int? SenderId { get; set; }
    public int? ReceiverId { get; set; }
    public int? DispatchId { get; set; }
    public string? Note { get; set; }

    // Navigation Properties
    public virtual TblFiscalPeriod? FiscalPeriod { get; set; }
    public virtual TblRecipient? Sender { get; set; }
    public virtual TblRecipient? Receiver { get; set; }
    public virtual TblUser? EnteredBy { get; set; }
    public virtual TblDispatch? Dispatch { get; set; }

    public virtual ICollection<TblArrivalLine> ArrivalLines { get; set; }
        = new List<TblArrivalLine>();

}
