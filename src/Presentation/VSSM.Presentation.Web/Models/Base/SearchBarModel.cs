using Microsoft.AspNetCore.Mvc.Rendering;

namespace VSSM.Presentation.Web.Models.Base;
public class SearchBarModel
{
    public string? SearchTerm { get; set; }
    public int PageSize { get; set; } = 10;
    public string Sort { get; set; } = string.Empty;
    public string Direction { get; set; } = "desc";
    public IEnumerable<SelectListItem>? FilterOptions { get; set; }
    public string FilterName { get; set; } = "status";
    public string? FilterValue { get; set; }
    public string AddController { get; set; } = string.Empty;
    public string AddAction { get; set; } = "New";
    public bool ShowAddButton { get; set; } = false;
}
