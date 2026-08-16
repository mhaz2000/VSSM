namespace VSSM.Infrastructure.Data.Models;

public partial class TblArrivalLine
{
    // Primary Key
    public int Id { get; set; }

    // Relationships
    public int? ArrivalId { get; set; }
    public int? ItemId { get; set; }
    public int? UseId { get; set; }
    public int? ManufacturerId { get; set; }
    public int? DosePerVialId { get; set; }
    public int? StoreId { get; set; }
    public int? ItemDetailId { get; set; }
    public int? CommercialNameId { get; set; }
    public int? VvmTypeId { get; set; }
    public int? VvmStageId { get; set; }

    // Quantities
    public int? Quantity { get; set; }
    public int? NumVials { get; set; }

    // Details
    public string? BatchNum { get; set; }
    public string? Note { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime? EntryDate { get; set; }
    public DateTime? ProductionDate { get; set; }

    // Navigation Properties
    public TblArrival? Arrival { get; set; }
    public TblItem? Item { get; set; }
    public TblBaseInfo? Use { get; set; }
    public TblBaseInfo? Manufacturer { get; set; }
    public TblBaseInfo? DosePerVial { get; set; }
    public TblStore? Store { get; set; }
    public TblItemDetail? ItemDetail { get; set; }
    public TblBaseInfo? CommercialName { get; set; }
    public TblBaseInfo? VvmType { get; set; }
    public TblBaseInfo? VvmStage { get; set; }

    public ICollection<TblAdjustment> Adjustments { get; set; }
        = new List<TblAdjustment>();
}
