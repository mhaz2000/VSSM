
namespace VSSM.Application.DTOs.BaseEntities.ItemDetail;

public record ToDispatchItemDetailDto
{
    public int? ItemId { get; internal set; }
    public string ItemName { get; internal set; }
    public DateTime? ExpiryDate { get; internal set; }
    public string VVMStep { get; internal set; }
    public string BatchNumber { get; internal set; }
    public string StoreName { get; internal set; }
    public long CurrentStock { get; set; }
    public string Manufacture { get; set; }
    public int StoreId { get; internal set; }
    public int ItemDetailStoreId { get; internal set; }
}
