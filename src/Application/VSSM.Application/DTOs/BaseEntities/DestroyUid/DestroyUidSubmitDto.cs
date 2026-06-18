namespace VSSM.Application.DTOs.BaseEntities.DestroyUid;

public class DestroyUidSubmitDto
{
    public int PkOrderInfo { get; set; }
    public int DestroyReasonId { get; set; }
    public string DestroyDate { get; set; } = null!;
    public string? SearchedUid { get; set; } 

}
