namespace VSSM.Application.DTOs.BaseEntities.Reports;

public record StoreEquipmentQuery
{
    public int? RecipientId { get; set; }
    public int? StoreTypeId { get; set; }
    public int? VolumeStart { get; set; }
    public int? VolumeEnd { get; set; }
    public int? FreeVolumeStart { get; set; }
    public int? FreeVolumeEnd { get; set; }
    public int? YearStart { get; set; }
    public int? YearEnd { get; set; }
    public int? IsActive { get; set; }
}