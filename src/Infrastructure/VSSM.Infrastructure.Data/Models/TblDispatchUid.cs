namespace VSSM.Infrastructure.Data.Models;

public partial class TblDispatchUid
{
    public int PKDispatchUid { get; set; }
    public int FKDispatchId { get; set; }
    public int FKUidInfo { get; set; }

    public virtual TblDispatch Dispatch { get; set; } = null!;
    public virtual TblUidInfo UidInfo { get; set; } = null!;


}
