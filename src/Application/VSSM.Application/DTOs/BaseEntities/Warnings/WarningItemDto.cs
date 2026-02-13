namespace VSSM.Application.DTOs.BaseEntities.Warnings;
public record WarningItemDto
{
    public string ItemName { get; set; }
    public long Quantity { get; set; }
    public long MinQuantity { get; set; }
    public long MaxQuantity { get; set; }
    public long OrderLevel { get; set; }
}
