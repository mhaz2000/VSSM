namespace VSSM.Application.DTOs.BaseEntities.Reports;

public record GroupedArrivalByItemDto
{
    public ArrivalByItemMetaDto Meta { get; init; }
    public List<ArrivalByItemDetailDto> Details { get; init; } = new();
}