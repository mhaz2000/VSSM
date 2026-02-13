namespace VSSM.Presentation.Web.Models.Base;

public class GridViewModel
{
    public PagedResult<object> Data { get; set; }

    public List<GridColumn> Columns { get; set; } = new();

    public string CurrentSort { get; set; }
    public string CurrentDirection { get; set; }

    public string SearchTerm { get; set; }

}
