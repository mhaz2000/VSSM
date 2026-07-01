namespace VSSM.Infrastructure.Data.Models;

public class TblRequestLine
{
    public int Id { get; set; }

    public int? RequestId { get; set; }
    public int? ItemId { get; set; }
    public int? ItemClassId { get; set; }

    public int? ArriveCount { get; set; }
    public int? UsedCount { get; set; }
    public int? CurrentCount { get; set; }

    public int? ReqCount { get; set; }

    public string Note { get; set; } = string.Empty;

    public int? ConfirmCount { get; set; }

    public int? DosePerVialId { get; set; }

    public virtual TblBaseInfo DosePerVial { get; set; } = default!;
    public virtual TblBaseInfo ItemClass { get; set; } = default!;
    public virtual TblItem Item { get; set; }= default!;
    public virtual TblRequest Request { get; set; } = default!;
}