namespace VSSM.Application.DTOs.BaseEntities.DestroyUid;

public class DestroyUidSearchResultDto
{
    public int PkOrderInfo { get; set; }
    public string BatchNumber { get; set; } = null!;
    public string ItemName { get; set; } = null!;
    public int DosePerVial { get; set; }
    public string ProductionDate { get; set; } = null!;
    public string ExpiryDate { get; set; } = null!;
    public int UidCount { get; set; }
}
