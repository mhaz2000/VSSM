namespace VSSM.Application.DTOs.BaseEntities.Dispatches;

public class DispatchPagingSearchOrderDto : PagingSearchOrderDto<DispatchListDto>
{
    public string? Status { get; set; }
}
