using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VSSM.Application.DTOs.BaseEntities.ItemClass;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Domain.Enums;
using VSSM.Presentation.Web.Models.ItemClass;

namespace VSSM.Presentation.Web.Controllers
{
    [Authorize]
    public class ItemClassController : BaseController
    {
        private readonly IItemClassService _itemClassService;
        public ItemClassController(IItemClassService itemClassService)
        {
            _itemClassService = itemClassService;
        }
        public async Task<IActionResult> Index(CancellationToken cancellationTOken, string? search,
                                                int page = 1,
                                                int pageSize = 10,
                                                string sortField = "Orders",
                                                string sortOrder = "desc")
        {
            var listModel = await _itemClassService.GetPagedList(search,
                                                                    page,
                                                                    pageSize,
                                                                    sortField,
                                                                    sortOrder,
                                                                    cancellationTOken);

            return View(listModel);
        }

        public IActionResult New()
        {
            LoadDropDowns();
            return View();
        }

        public async Task<IActionResult> Create(ItemClassViewModel model, CancellationToken cancellationTOken)
        {
            if (!ModelState.IsValid)
            {
                return View("New", model);
            }

            var dto = new ItemClassCreateDto
            {
                ItemClassId = model.ItemClassId.GetValueOrDefault(),
                Code = model.Code,
                FullName = model.FullName,
                IsActive = model.IsActive,
                OrderInList = model.OrderInList,
                ReqBatchNo = model.ReqBatchNo,
                ReqExpireDate = model.ReqExpireDate,
                ReqManufacturer = model.ReqManufacturer,
                UnitName = model.UnitName.ToString(),
                ReqDosePerVial = model.ReqDosePerVial,
            };

            if (model.ItemClassId == null || model.ItemClassId == 0)
            {
                // ---------- Create ----------
                var res = await _itemClassService.Create(dto, cancellationTOken);
            }
            else
            {
                // ---------- Update ----------
                var res = await _itemClassService.Update(dto, cancellationTOken);
            }

            TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
            TempData["ToastType"] = "success";

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id, CancellationToken cancellationTOken)
        {
            // داده را از دیتابیس می‌خوانیم
            var itemClass = await _itemClassService.GetById(id, cancellationTOken);
            if (itemClass == null)
                return NotFound();

            var model = new ItemClassViewModel
            {
                ItemClassId = itemClass.ItemdClassId,
                Code = itemClass.Code,
                FullName = itemClass.FullName,
                IsActive = itemClass.IsActive,
                OrderInList = itemClass.OrderInList,
                ReqBatchNo = itemClass.ReqBatchNo.GetValueOrDefault(),
                ReqExpireDate = itemClass.ReqExpiryDate.GetValueOrDefault(),
                ReqManufacturer = itemClass.ReqManufacturer.GetValueOrDefault(),
                UnitName = Enum.Parse<UnitNameEnum>(itemClass.UnitName),
                ReqDosePerVial = itemClass.ReqDosePerVial.GetValueOrDefault(),
            };

            LoadDropDowns();

            // نمایش فرم ویرایش با داده‌ها
            return View("New", model);
        }


        private void LoadDropDowns()
        {
            ViewBag.UnitList = Enum.GetValues(typeof(UnitNameEnum))
                                .Cast<UnitNameEnum>()
                                .Select(u => new SelectListItem
                                {
                                    Text = u.ToString(),
                                    Value = u.ToString()
                                })
                                .ToList();
        }

    }
}
