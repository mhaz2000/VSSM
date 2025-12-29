using Microsoft.AspNetCore.Mvc.Rendering;
using VSSM.Application.Helpers;

namespace VSSM.Presentation.Web.Models.Adjustment;

public class AdjustmentViewModel
{
    public int AdjustmentId { get; set; }
    public int? AdjustmentreasonId { get; set; }
    public int? FiscalPeriodId { get; set; }
    public int? ItemClassId { get; set; }
    public int? ArrivalLineId { get; set; }
    public int? DispatchLineId { get; set; }
    public int? ItemDetailStoreId { get; set; }
    public int? ItemId { get; set; }
    public int? OldQuantity { get; set; }
    public int? NewQuantity { get; set; }
    public int? EnteredById { get; set; }
    public DateTime? EntryDate { get; set; }
    public DateTime? Date { get; set; }
    public string EntryShamsiDate => Date.HasValue ? Date.Value.ToShamsi() : string.Empty;
    public string? Note { get; set; } = "";
}