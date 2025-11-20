namespace VSSM.Presentation.Web.Models.AdjustReason;

public class AdjustReasonViewModel
{
    public int? AdjustReasonId { get; set; } // nullable for new/edit

    public string Code { get; set; } = "";
    public string FullName { get; set; } = "";
    public DateTime? EntryDate { get; set; }

    // ✅ All checkboxes MUST be non-nullable bool
    public bool IsActive { get; set; } = false;
    public bool UsedForDispatchAdjust { get; set; } = false;
    public bool UserForArrivalAdjust { get; set; } = false;
    public bool UsedForStockAdjust { get; set; } = false;
    public bool IsWaste { get; set; } = false;
    public bool ApplicationForStockSubtraction { get; set; } = false;
    public bool ApplicationForStockAddition { get; set; } = false;
}