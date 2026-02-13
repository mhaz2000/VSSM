namespace VSSM.Application.DTOs.BaseEntities.Warnings;

public record LowItemsMinDto : WarningItemDto
{
    public long Difference => Quantity - MinQuantity;
}
