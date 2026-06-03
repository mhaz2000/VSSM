using Microsoft.AspNetCore.Mvc.RazorPages;
using VSSM.Application.DTOs;

namespace VSSM.Presentation.Web.Models.Base;

public class GridViewModel
{
    public PagedResult<object> Data { get; set; }

    public List<GridColumn> Columns { get; set; } = new();
    public List<GridAction> Actions { get; set; } = new();
    public string CurrentSort { get; set; } = "Id";
    public string CurrentDirection { get; set; } = "desc";

    public string SearchTerm { get; set; }

}

public class GroupedGridViewModel<TMeta, TDetail>
{
    public List<GroupItem<TMeta, TDetail>> Groups { get; set; } = new();

    public PagedResult<object> Data { get; set; }

    public List<GridColumn> Columns { get; set; } = new();

    public string CurrentSort { get; set; }
    public string CurrentDirection { get; set; }
}

public class GroupItem<TMeta, TDetail>
{
    public TMeta Meta { get; set; }
    public List<TDetail> Details { get; set; } = new();
}
