namespace VSSM.Application.DTOs.Reports;

public record PhysicalInventoryQuery
{
    public long? ItemClassId { get; set; }
    public long? ItemId { get; set; }
    public long? SourceId { get; set; }
    public long? StoreId { get; set; }
}