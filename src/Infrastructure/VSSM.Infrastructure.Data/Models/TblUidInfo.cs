namespace VSSM.Infrastructure.Data.Models;
public partial class TblUidInfo
{
    public int PkUidInfo { get; set; }

    public string PatchBarcode { get; set; } = string.Empty;

    public string Barcode { get; set; } = string.Empty;

    public string HashCode { get; set; } = string.Empty;

    public string FirstLevelUid { get; set; } = string.Empty;

    public string SecLevelUid { get; set; } = string.Empty;

    public string ParentUid { get; set; } = string.Empty;

    public bool Used { get; set; } = false;

    public int? CurrentStore { get; set; } = 0;
    public int? ConsumedCount { get; set; } = 0;

    public bool IsActive { get; set; }

    public int FkOrderInfo { get; set; }

    public int? DoseCount { get; set; }
    public int? FKDispatchUid { get; set; }

    public int? FkReceiveDispatchId { get; set; }
    public virtual TblReceiveDispatches ReceiveDispatch { get; set; }


    public virtual TblOrderInfo OrderInfo { get; set; } = null!;

    public virtual TblDispatchUid DispatchUid { get; set; } = null!;

    public int? FkDestroyId { get; set; }
    public virtual TblDestroyUid? DestroyUid { get; set; }
}
