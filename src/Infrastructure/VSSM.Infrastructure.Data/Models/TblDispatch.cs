namespace VSSM.Infrastructure.Data.Models;

public partial class TblDispatch
{
    public int DispatchId { get; set; }

    // Foreign Keys
    public int? SenderId { get; set; }
    public int? ReceiverId { get; set; }
    public int? FiscalPeriodId { get; set; }
    public int? EnteredById { get; set; }
    public int? ArrivalId { get; set; }
    public int? ReceiverStatus { get; set; }
    public int? FkReceiveDispatchId { get; set; }

    // Dispatch Information
    public string? TransportMode { get; set; }
    public string? RequestNum { get; set; }
    public string? DispatchVoucherNumber { get; set; }
    public string? ReceivingMode { get; set; }
    public string? ReportNumber { get; set; }
    public string? Note { get; set; }
    public string? CancelReason { get; set; }

    // Status
    public bool? IsConfirmed { get; set; }
    public bool? IsCancelled { get; set; }
    public bool? DispatchVoucherPrinted { get; set; }
    public bool? IsDrafted { get; set; }
    public bool? IsDel { get; set; }

    // Dates
    public DateTime? ConfirmationTime { get; set; }
    public DateTime? Date { get; set; }
    public DateTime? EntryDate { get; set; }

    // Sequence
    public long SeqPreview { get; set; }

    // Navigation Properties
    public virtual TblReceiveDispatches? ReceiveDispatch { get; set; }
    public virtual TblArrival? Arrival { get; set; }
    public virtual TblUser? EnteredBy { get; set; }
    public virtual TblFiscalPeriod? FiscalPeriod { get; set; }
    public virtual TblRecipient? Receiver { get; set; }
    public virtual TblRecipient? Sender { get; set; }

    // Collections
    public virtual ICollection<TblDispatchLine> TblDispatchLines { get; set; }
        = new List<TblDispatchLine>();
}