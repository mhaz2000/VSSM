namespace VSSM.Application.DTOs.BaseEntities.AdjustReason;

public class AdjustreasonDto
{
    public int AdjustReasonId { get; set; }

    public string Code { get; set; } = "";
    public string FullName { get; set; } = "";
    public DateTime? EntryDate { get; set; } = DateTime.Now;

    public bool IsActive { get; set; }
    public bool UsedForDispatchAdjust { get; set; }
    public bool UserForArrivalAdjust { get; set; }
    public bool UsedForStockAdjust { get; set; }
    public bool IsWaste { get; set; }
    public bool ApplicationForStockSubtraction { get; set; }
    public bool ApplicationForStockAddition { get; set; }
}
