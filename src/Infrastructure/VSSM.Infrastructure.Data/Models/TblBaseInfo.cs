namespace VSSM.Infrastructure.Data.Models;

public partial class TblBaseInfo
{
    public int BaseInfoId { get; set; }
    public string? Code { get; set; }
    public string Title { get; set; }
    public int? ParentId { get; set; }
    public int? Order { get; set; }
    public DateTime CreateDate { get; set; }
    public string CreateBy { get; set; }
    public string UpdateBy { get; set; }
    public DateTime UpdateDate { get; set; }

    public virtual TblBaseInfo? Parent { get; set; }
    public virtual ICollection<TblBaseInfo> Children { get; set; } = new List<TblBaseInfo>();
}
