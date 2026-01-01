namespace VSSM.Application.DTOs.BaseEntities.ItemDetail;

public class ItemFullDetailsDto
{
    public int ItemDetailId { get; set; }
    public int? ItemId { get; set; }
    public string BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public float? UnitPrice { get; set; }
    public DateTime? ProductionDate { get; set; }
    public float? UnitVolume { get; set; }
    public DateTime? EntryDate { get; set; }
    public bool? IsConfirmed { get; set; }
}
