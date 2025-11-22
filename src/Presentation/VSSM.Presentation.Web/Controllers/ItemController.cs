using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using VSSM.Application.Common.Extensions;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Domain.Enums;

namespace VSSM.Presentation.Web.Controllers
{
    public class ItemController : Controller
    {
        private readonly IItemService _itemService;
        private readonly IItemClassService _itemClassService;
        public ItemController(IItemService itemService, IItemClassService itemClassService)
        {
            _itemService = itemService;
            _itemClassService = itemClassService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken, string? search,
                                                int page = 1,
                                                int pageSize = 10,
                                                string sortField = "Orders",
                                                string sortOrder = "desc")
        {
            var listModel = await _itemService.GetPagedList(search,
                                                                    page,
                                                                    pageSize,
                                                                    sortField,
                                                                    sortOrder,
                                                                    cancellationToken);

            await PrepareDropdown(cancellationToken);
            return View(listModel);

        }
        public async Task<IActionResult> New()
        {
            await PrepareDropdown(CancellationToken.None);
            return View();
        }

        private async Task PrepareDropdown(CancellationToken cancellationTOken)
        {
            var list = await _itemClassService.GetActives(cancellationTOken);

            ViewBag.ItemClass = list;
        }
    }
}
