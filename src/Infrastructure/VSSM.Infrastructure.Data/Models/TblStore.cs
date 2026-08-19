namespace VSSM.Infrastructure.Data.Models;

public partial class TblStore
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public int? TypeId { get; set; }
    public float? FreeVolume { get; set; }
    public float? Volume { get; set; }
    public string Note { get; set; }
    public bool? IsActive { get; set; }
    public int? RecipientId { get; set; }
    public int? Year { get; set; }
    public virtual TblRecipient Recipient { get; set; }
    public virtual TblBaseInfo Type { get; set; }

    public virtual ICollection<TblDispatchLine> DispatchLines { get; set; } = new List<TblDispatchLine>();
    public virtual ICollection<TblItemDetailStore> ItemDetailStores { get; set; } = new List<TblItemDetailStore>();
}