using VSSM.Domain.Enums;

namespace VSSM.Infrastructure.Data.Models;

public class TblRequest
{
    public int Id { get; set; }

    public int? SenderId { get; set; }

    public DateOnly? SendDate { get; set; } = new DateOnly();

    public RequestStatusEnum? Status { get; set; }

    public int? ReceiverId { get; set; }

    public int? EnteredBy { get; set; }

    public DateOnly? ConfirmDate { get; set; } = null;

    public string RequestNumber { get; set; } = string.Empty;

    public int? FiscalPeriodId { get; set; }

    public virtual TblUser EnteredByNavigation { get; set; } = default!;
    public virtual TblFiscalPeriod FiscalPeriod { get; set; } = default!;
    public virtual TblRecipient Receiver { get; set; } = default!;
    public virtual TblRecipient Sender { get; set; } = default!;

    public virtual ICollection<TblRequestLine> TblRequestLines { get; set; } = [];
}
