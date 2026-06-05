namespace VSSM.Infrastructure.Data.Models;

public class TblReceiveDispatches
{
    public int PkReceiveDispatchesId { get; set; }
    public DateTime ReceiveDate { get; set; }
    public int ReceiveNumber { get; set; }
    public DateTime CreationTime { get; set; }
    public int CreatedBy { get; set; }

    public virtual TblUser CreatedByUser { get; set; }
    public virtual ICollection<TblUidInfo> UidInfos { get; set; }
    public virtual ICollection<TblDispatch> Dispatches { get; set; }
}
