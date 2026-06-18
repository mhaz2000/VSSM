namespace VSSM.Application.DTOs.BaseEntities.DestroyUid;

public class DestroyUidSearchResultDto
{
    public int PkOrderInfo { get; set; }
    public string? SearchedUid { get; set; } 
    public string? BatchNumber { get; set; }
    public string? ItemName { get; set; }
    public int? DosePerVial { get; set; }
    public string? ProductionDate { get; set; }
    public string? ExpiryDate { get; set; }
    public int UidCount { get; set; }
}
