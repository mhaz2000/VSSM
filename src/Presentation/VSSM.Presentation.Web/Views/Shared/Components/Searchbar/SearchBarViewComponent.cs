using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VSSM.Presentation.Web.Models.Base;

namespace VSSM.Presentation.Web.Views.Shared.Components.Searchbar;

public class SearchBarViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(
        string? searchTerm = null,
        int pageSize = 10,
        string sort = "",
        string direction = "desc",
        IEnumerable<SelectListItem>? filterOptions = null,
        string filterName = "status",
        string? filterValue = null,
        string addController = "",
        string addAction = "New",
        bool showAddButton = false)
    {
        var model = new SearchBarModel
        {
            SearchTerm = searchTerm,
            PageSize = pageSize,
            Sort = sort,
            Direction = direction,
            FilterOptions = filterOptions,
            FilterName = filterName,
            FilterValue = filterValue,
            AddController = addController,
            AddAction = addAction,
            ShowAddButton = showAddButton,
        };

        return View(model);
    }
}