namespace VSSM.Application.DTOs.BaseEntities.Dispatches;

public class DispatchLineDto
{
    public int ItemId { get; set; }
    public int Quantity { get; set; }
    public float UnitPrice { get; set; }
    public int? NumVial { get; set; }
    public int? DosePerVialId { get; set; }
    public float? TotalVolume { get; set; }
    public int? StoreId { get; set; }
    public int? UseId { get; set; }
    public int? VvmtypeId { get; set; }
    public int? VvmstageId { get; set; }
    public int? ManufacturerId { get; set; }
    public int? ItemDetailStoreId { get; set; }
    public string Note { get; set; }
    public string BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? BundleId { get; set; }
}
