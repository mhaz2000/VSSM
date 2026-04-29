namespace VSSM.Application.DTOs.BaseEntities.Reports;
public record AvailableQuantityReportQuery
{
    public int? ItemClassId { get; set; }
    public string ItemId { get; set; }
    public string StoreId { get; set; }

    public int? SourceId { get; set; }
    public int? Level { get; set; }
    public int? RecipientId { get; set; }
    public int? CommercialNameId { get; set; }
    public int? VvmstageId { get; set; }
    public int? VvmtypeId { get; set; }

    public int? ManufacturerId { get; set; }

    public string? BatchNumber { get; set; }

    public string? ProductinDate { get; set; }
    public string? ExpiryDate { get; set; }
    public bool AllSubRecipientStocks { get; set; }
}
