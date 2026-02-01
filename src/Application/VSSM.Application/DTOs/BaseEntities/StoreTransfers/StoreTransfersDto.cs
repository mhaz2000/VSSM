namespace VSSM.Application.DTOs.BaseEntities.StoreTransfers;

public class StoreTransfersDto
{
    public int ToStoreId { get; set; }
    public List<TransferItem> Items { get; set; } = new();

}

public class TransferItem
{
    public int ItemDetailStoreId { get; set; }
    public int Quantity { get; set; }
}
