namespace VSSM.Infrastructure.Data.Models;

public partial class TblUidInfo
{
    public int PkUidInfo { get; set; }

    public string PatchBarcode { get; set; } = null!;

    public string Barcode { get; set; } = null!;

    public string HashCode { get; set; } = null!;

    public string ChildUid { get; set; } = null!;

    public string ParentUid { get; set; } = null!;

    public bool IsActive { get; set; }

    public int FkOrderInfo { get; set; }

    public int? FkArrivalId { get; set; }
    public int? FkItemId { get; set; }
    public int? CurrentStore { get; set; }
    public int? DoseCount { get; set; }

    public virtual TblArrival? FkArrival { get; set; }
    public virtual TblItem? FkItem { get; set; }
    public virtual TblOrderInfo OrderInfo { get; set; } = null!;
}
