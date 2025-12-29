using VSSM.Application.Helpers;

namespace VSSM.Application.DTOs.BaseEntities.Adjustments;

public class AdjustmentDto
{
    public int AdjustmentId { get; set; }
    public int? AdjustmentreasonId { get; set; }
    public int? FiscalPeriodId { get; set; }
    public int? ArrivalLineId { get; set; }
    public int? DispatchLineId { get; set; }
    public int? ItemDetailStoreId { get; set; }
    public int? OldQuantity { get; set; }
    public int? NewQuantity { get; set; }
    public int? EnteredById { get; set; }
    public DateTime? EntryDate { get; set; }
    public DateTime? Date { get; set; }
    public string EntryShamsiDate => Date.HasValue ? Date.Value.ToShamsi() : string.Empty;
    public string Note { get; set; } = "";
}