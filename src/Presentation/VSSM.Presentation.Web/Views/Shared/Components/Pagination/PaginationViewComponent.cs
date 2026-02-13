using Microsoft.AspNetCore.Mvc;
using VSSM.Presentation.Web.Models.Base;

namespace VSSM.Presentation.Web.Views.Shared.Components.Pagination;

public class PaginationViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(
    PagedResult<object> model,
    string sort,
    string direction,
    string search = "")
    {
        var vm = new PaginationModel
        {
            Data = model,
            Sort = sort,
            Direction = direction,
            Search = search
        };

        return View(vm);
    }

}

