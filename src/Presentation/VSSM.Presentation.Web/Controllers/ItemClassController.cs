using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.ItemClass;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Application.Services.BaseEntities;
using VSSM.Presentation.Web.Models.ItemClass;

namespace VSSM.Presentation.Web.Controllers
{
    public class ItemClassController : Controller
    {
        private readonly IItemClassService _itemClassService;
        public ItemClassController(IItemClassService itemClassService)
        {
            _itemClassService = itemClassService;
        }
        public async Task<IActionResult> Index(CancellationToken cancellationTOken,string? search,
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
                ItemdClassId = model.ItemdClassId,
                Code = model.Code,
                FullName = model.FullName,
                IsActive = model.IsActive,
                OrderInList = model.OrderInList,
                ReqBatchNo = model.ReqBatchNo,
                ReqExpireDate = model.ReqExpireDate,
                ReqManufacturer = model.ReqManufacturer,
                UnitName = model.UnitName,
                ReqDosePerVial = model.ReqDosePerVial,
            };

            if (model.ItemdClassId == 0)
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
                ItemdClassId = itemClass.ItemdClassId,
                Code = itemClass.Code,
                FullName = itemClass.FullName,
                IsActive = itemClass.IsActive,
                OrderInList = itemClass.OrderInList,
                ReqBatchNo = itemClass.ReqBatchNo.GetValueOrDefault(),
                ReqExpireDate = itemClass.ReqExpiryDate.GetValueOrDefault(),
                ReqManufacturer = itemClass.ReqManufacturer.GetValueOrDefault(),
                UnitName = itemClass.UnitName,
                ReqDosePerVial = itemClass.ReqDosePerVial.GetValueOrDefault(),
            };

            // نمایش فرم ویرایش با داده‌ها
            return View("New", model);
        }

    }
}
