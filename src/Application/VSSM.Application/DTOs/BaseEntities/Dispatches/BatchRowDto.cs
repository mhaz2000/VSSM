namespace VSSM.Application.DTOs.BaseEntities.Dispatches;

public class BatchRowDto
{
    public int ItemDetailStoreId { get; set; }
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? BatchNumber { get; set; }

    // بند ۷a: تاریخ انقضا شمسی
    public string? ExpiryDateShamsi { get; set; }
    public DateTime? ExpiryDate { get; set; }

    // بند ۱۰: موجودی جاری
    public long CurrentStock { get; set; }
    public string DosePerVial { get; set; } = "-";
    public int DosePerVialNum { get; set; }
    public string ManufacturerName { get; set; } = "-";
    public int StoreId { get; set; }
    public string StoreName { get; set; } = "-";

    // بند ۶: اطلاعات حلال (RequiresDiluent)
    public bool RequiresDiluent { get; set; }
    public int? DiluentItemDetailStoreId { get; set; }
    public int? DiluentItemId { get; set; }
    public string? DiluentItemName { get; set; }
    public string? DiluentBatchNumber { get; set; }
    public string? DiluentExpiryDateShamsi { get; set; }
    public DateTime? DiluentExpiryDate { get; set; }
    public string? DiluentManufacturerName { get; set; }

    public bool IsDiluent { get; set; }

}
