using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.Vvmstage;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.Vvmstage;

namespace VSSM.Presentation.Web.Controllers;


public class VvmstageController : BaseController
    {
        private readonly IVvmstageService _vvmstageService;

        public VvmstageController(IVvmstageService vvmstageService)
        {
            _vvmstageService = vvmstageService;
        }

        public async Task<IActionResult> Index(
            CancellationToken cancellationToken,
            string? search = null,
            int page = 1,
            int pageSize = 10,
            string sortField = "VvmstageId",
            string sortOrder = "asc")
        {
            var vvmstages = await _vvmstageService.GetPagedList(
                search,
                page,
                pageSize,
                sortField,
                sortOrder,
                cancellationToken);

            return View(vvmstages);  
        }

        public IActionResult New()
        {
            return View(new VvmstageViewModel()); 
        }

        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var vvmstage = await _vvmstageService.GetById(id, cancellationToken);
            if (vvmstage == null)
                return NotFound();

            var model = new VvmstageViewModel
            {
                VvmstageId = vvmstage.VvmstageId,
                Code = vvmstage.Code,
                FullName = vvmstage.FullName,
                EntryDate = vvmstage.EntryDate,
                IsActive = vvmstage.IsActive
            };

            return View("New", model); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VvmstageViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View("New", model);
            }

            var dto = new VvmstageDto
            {
                VvmstageId = model.VvmstageId.GetValueOrDefault(),
                Code = model.Code,
                FullName = model.FullName,
                EntryDate = DateTime.Now,
                IsActive = model.IsActive
            };

            if (model.VvmstageId == null || model.VvmstageId == 0)
            {
                await _vvmstageService.Create(dto, cancellationToken);
            }
            else
            {
                await _vvmstageService.Update(dto, cancellationToken);
            }

            TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
            TempData["ToastType"] = "success";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var deleted = await _vvmstageService.Delete(id, cancellationToken);

            if (deleted)
            {
                TempData["ToastMessage"] = "عملیات حذف با موفقیت انجام شد";
                TempData["ToastType"] = "success";
            }
            else
            {
                TempData["ToastMessage"] = "مرحله مورد نظر یافت نشد";
                TempData["ToastType"] = "error";
            }

            return RedirectToAction("Index");
        }
    }