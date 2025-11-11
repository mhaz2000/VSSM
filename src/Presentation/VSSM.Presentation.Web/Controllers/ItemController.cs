using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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

        public async Task<IActionResult> Index(CancellationToken cancellationTOken, string? search,
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
                                                                    cancellationTOken);

            LoadItemTypeEnum();
            return View(listModel);
        
        }
        public IActionResult New()
        {
            LoadItemTypeEnum();
            return View();
        }

        private void LoadItemTypeEnum()
        {
            var list = Enum.GetValues(typeof(ItemTypeEnum))
                   .Cast<ItemTypeEnum>()
                   .Select(x => new
                   {
                       Id = (int)x,
                       Title = x.GetDescription()
                   })
                   .ToList();

            ViewBag.ItemTypes = list;
        }
    }
}
