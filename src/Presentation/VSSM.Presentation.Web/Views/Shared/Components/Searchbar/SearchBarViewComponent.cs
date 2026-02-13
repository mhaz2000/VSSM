using Microsoft.AspNetCore.Mvc;
using VSSM.Presentation.Web.Models.Base;

namespace VSSM.Presentation.Web.Views.Shared.Components.Searchbar;

public class SearchBarViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(
        string searchTerm,
        int pageSize,
        string sort,
        string direction)
    {
        var model = new SearchBarModel
        {
            SearchTerm = searchTerm,
            PageSize = pageSize,
            Sort = sort,
            Direction = direction
        };

        return View(model);
    }
}