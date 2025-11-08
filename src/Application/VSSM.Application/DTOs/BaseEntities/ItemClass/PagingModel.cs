namespace VSSM.Application.DTOs.BaseEntities.ItemClass;

public class PagingModel
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

    public int Skip => (Page - 1) * PageSize;
}
