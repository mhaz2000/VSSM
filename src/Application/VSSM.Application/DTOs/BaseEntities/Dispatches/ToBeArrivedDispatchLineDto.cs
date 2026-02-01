namespace VSSM.Application.DTOs.BaseEntities.Dispatches;

public class ToBeArrivedDispatchLineDto
{
    public int? ItemId { get; set; }
    public string ItemName { get; set; }
    public int DispatchLineId { get; set; }
    public string Manufacure { get; set; }
    public int? DoesPerVial { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string BatchNumber { get; set; }
    public int? DoseCount { get; set; }
    public string Vvmstage { get; set; }
    public bool? UserBelowZeroStore { get; internal set; }
    public bool? UserAboveZeroStore { get; internal set; }
    public bool? UseOrdinaryStore { get; internal set; }
}
