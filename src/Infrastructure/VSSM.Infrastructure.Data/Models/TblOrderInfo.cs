namespace VSSM.Infrastructure.Data.Models;

public partial class TblOrderInfo
{
    public int PkOrderInfo { get; set; }

    public int NumberOrder { get; set; }

    public DateTime ManufactureDate { get; set; }

    public DateTime? ExpirationDate { get; set; }

    public int? FkArrivalId { get; set; }
    public int FkItemId { get; set; }
    public int FkDosePerVialId { get; set; }

    public string Irc { get; set; } = null!;

    public string BatchNumber { get; set; } = null!;

    public string? DistributorCompanyCode { get; set; }

    public string? OrderCompanyCode { get; set; }

    public string? LicenseCode { get; set; }

    public string? Prefix { get; set; }

    public DateTime CreationTime { get; set; }

    public int CreatedBy { get; set; }

    public string Status { get; set; }

    public string FileName { get; set; } = null!;


    public virtual TblItem Item { get; set; }
    public virtual TblArrival Arrival { get; set; }
    public virtual TblBaseInfo DosePerVial  { get; set; }

    public virtual ICollection<TblUidInfo> TblUidInfos { get; set; }
        = new List<TblUidInfo>();

    public virtual ICollection<TblUidError> TblUidErrors { get; set; }
    = new List<TblUidError>();
}
