using Humanizer;

namespace VSSM.Infrastructure.Data.Models;


public partial class TblUidError
{
    public int PkUidError { get; set; }
    public string FkUidInfo { get; set; } = null!;
    public string ErrorCode { get; set; } = null!;
    public string ErrorMessage { get; set; } = null!;

    public int FkOrderInfo { get; set; }

    public virtual TblOrderInfo OrderInfo { get; set; } = null!;

}
