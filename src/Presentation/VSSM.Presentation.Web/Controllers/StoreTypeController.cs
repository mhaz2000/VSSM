using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.StoreType;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.StoreType;

namespace VSSM.Presentation.Web.Controllers;

public class StoreTypeController : Controller
    {
        private readonly IStoreTypeService _storeTypeService;

        public StoreTypeController(IStoreTypeService storeTypeService)
        {
            _storeTypeService = storeTypeService;
        }

        public async Task<IActionResult> Index(
            CancellationToken cancellationToken,
            string? search = null,
            int page = 1,
            int pageSize = 10,
            string sortField = "StoreTypeId",
            string sortOrder = "asc")
        {
            var storeTypes = await _storeTypeService.GetPagedList(
                search,
                page,
                pageSize,
                sortField,
                sortOrder,
                cancellationToken);

            return View(storeTypes); 
        }

        public IActionResult New()
        {
            return View(new StoreTypeViewModel()); 
        }

        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var storeType = await _storeTypeService.GetById(id, cancellationToken);
            if (storeType == null)
                return NotFound();

            var model = new StoreTypeViewModel
            {
                StoreTypeId = storeType.StoreTypeId,
                Code = storeType.Code,
                FullName = storeType.FullName,
                EntryDate = storeType.EntryDate,
                IsActive = storeType.IsActive,
                IsUserEditable = storeType.IsUserEditable
            };

            return View("New", model); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StoreTypeViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View("New", model);
            }

            var dto = new StoreTypeDto
            {
                StoreTypeId = model.StoreTypeId.GetValueOrDefault(),
                Code = model.Code,
                FullName = model.FullName,
                EntryDate = model.EntryDate,
                IsActive = model.IsActive,
                IsUserEditable = model.IsUserEditable
            };

            if (model.StoreTypeId == null || model.StoreTypeId == 0)
            {
                await _storeTypeService.Create(dto, cancellationToken);
            }
            else
            {
                await _storeTypeService.Update(dto, cancellationToken);
            }

            TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
            TempData["ToastType"] = "success";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var deleted = await _storeTypeService.Delete(id, cancellationToken);

            if (deleted)
            {
                TempData["ToastMessage"] = "عملیات حذف با موفقیت انجام شد";
                TempData["ToastType"] = "success";
            }
            else
            {
                TempData["ToastMessage"] = "نوع فروشگاه مورد نظر یافت نشد"; 
                TempData["ToastType"] = "error";
            }

            return RedirectToAction("Index");
        }
    }