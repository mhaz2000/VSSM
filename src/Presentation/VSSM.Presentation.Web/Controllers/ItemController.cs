using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using VSSM.Application.Common.Extensions;
using VSSM.Application.DTOs.BaseEntities.Item;
using VSSM.Application.DTOs.BaseEntities.ItemClass;
using VSSM.Application.DTOs.BaseEntities.ItemDetail;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Application.Shared;
using VSSM.Domain.Enums;
using VSSM.Presentation.Web.Models.Item;
using VSSM.Presentation.Web.Models.ItemClass;

namespace VSSM.Presentation.Web.Controllers
{
    [Authorize]
    public class ItemController : BaseController
    {
        private readonly IItemService _itemService;
        private readonly IItemClassService _itemClassService;
        private readonly IItemDetailsService _itemDetailsService;
        public ItemController(IItemService itemService, IItemClassService itemClassService, IItemDetailsService itemDetailsService)
        {
            _itemService = itemService;
            _itemClassService = itemClassService;
            _itemDetailsService = itemDetailsService;
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

        public async Task<IActionResult> Create(ItemViewModel model, CancellationToken cancellationTOken)
        {
            if (!ModelState.IsValid)
            {
                return Redirect("New");
            }

            var dto = new ItemCreateDto
            {
                ItemId = model.ItemId.GetValueOrDefault(),
                ItemClassId = model.ItemClassId,
                Code = model.Code,
                FullName = model.FullName,
                IsActive = model.IsActive,
                OrderId = model.OrderInList,
                DayToWarmExpire = model.DayToWarmExpire,
                DiluentId = model.DiluentId,
                RequiresDiluent = model.RequiresDiluent,
                UniCode = model.UniCode,
                UnitPrice = model.UnitPrice,
                UseOrdinaryStore = model.UseOrdinaryStore,
                UserAboveZeroStore = model.UserAboveZeroStore,
                UserBelowZeroStore = model.UserBelowZeroStore,
                Volume = model.Volume
            };

            if (model.ItemId == null || model.ItemId == 0)
            {
                // ---------- Create ----------
                var res = await _itemService.Create(dto, cancellationTOken);
            }
            else
            {
                // ---------- Update ----------
                var res = await _itemService.Update(dto, cancellationTOken);
            }

            TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
            TempData["ToastType"] = "success";

            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            // داده را از دیتابیس می‌خوانیم
            var item = await _itemService.GetById(id, cancellationToken);
            if (item == null)
                return NotFound();

            var model = new ItemViewModel
            {
                ItemId = item.ItemId,
                ItemClassId = item.ItemClassId.GetValueOrDefault(),
                Code = item.Code,
                FullName = item.FullName,
                IsActive = item.IsActive.GetValueOrDefault(),
                OrderInList = item.OrderId.GetValueOrDefault(),
                Volume = item.Volume.GetValueOrDefault(),
                RequiresDiluent = item.RequiresDiluent.GetValueOrDefault(),
                UserBelowZeroStore = item.UserBelowZeroStore.GetValueOrDefault(),
                UseOrdinaryStore = item.UseOrdinaryStore.GetValueOrDefault(),
                UnitPrice = item.UnitPrice.GetValueOrDefault(),
                UniCode = item.UniCode,
                DayToWarmExpire = item.DayToWarmExpire.GetValueOrDefault(),
                DiluentId = item.DiluentId.GetValueOrDefault(),
                UserAboveZeroStore = item.UserAboveZeroStore.GetValueOrDefault(),

            };

            await PrepareDropdown(cancellationToken);
            // نمایش فرم ویرایش با داده‌ها
            return View("New", model);
        }

        private async Task PrepareDropdown(CancellationToken cancellationTOken)
        {
            var list = await _itemClassService.GetActives(cancellationTOken);

            ViewBag.ItemClass = list;
        }

        [HttpGet("GetItems/{classItemId:required:int}")]
        public async Task<IActionResult> GetItems(int classItemId)
        {
            var apiResult = new ApiResult<List<ItemListDto>>();

            try
            {
                var items = await _itemService.GetItemsByClassId(classItemId);

                apiResult.Success = true;
                apiResult.Message = string.Empty;
                apiResult.Result = items;
            }
            catch (Exception ex)
            {

                throw;
            }

            return Json(apiResult);
        }

        [HttpGet("GetItemDetails/{itemId:required:int}")]
        public async Task<IActionResult> GetItemDetails(int itemId)
        {
            var apiResult = new ApiResult<List<ItemFullDetailsDto>>();

            try
            {
                var itemDetails = await _itemDetailsService.GetItemDetailsAsync(itemId, CancellationToken.None);

                apiResult.Success = true;
                apiResult.Message = string.Empty;
                apiResult.Result = itemDetails;
            }
            catch (Exception ex)
            {
            }

            return Json(apiResult);
        }
    }
}
