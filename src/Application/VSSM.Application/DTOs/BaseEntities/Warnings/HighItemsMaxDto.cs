namespace VSSM.Application.DTOs.BaseEntities.Warnings;

public record HighItemsMaxDto : WarningItemDto
{
    public long Difference => MaxQuantity - Quantity;
}
