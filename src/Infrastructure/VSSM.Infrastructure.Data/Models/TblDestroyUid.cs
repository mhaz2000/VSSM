namespace VSSM.Infrastructure.Data.Models;

public class TblDestroyUid
{
    public int PkDestroyUid { get; set; }
    public int FkUid { get; set; }
    public string DestroyDate { get; set; } = null!;
    public int DestroyNumber { get; set; }
    public int FkLocationDestroy { get; set; }
    public int FkDestroyeBy { get; set; }
    public int FkDestroyReasonId { get; set; }
    public DateTime CreationTime { get; set; }
    public int FkCreatedBy { get; set; }

    // Navigation Properties
    public virtual TblUidInfo UidInfo { get; set; } = null!;
    public virtual TblRecipient Location { get; set; } = null!;
    public virtual TblUser DestroyedByUser { get; set; } = null!;
    public virtual TblBaseInfo DestroyReason { get; set; } = null!;
    public virtual TblUser CreatedByUser { get; set; } = null!;
}
