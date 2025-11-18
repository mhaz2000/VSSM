using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.Vvmstage;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.Vvmstage;

namespace VSSM.Presentation.Web.Controllers;

 public class VvmstageController : Controller
    {
        private readonly IVvmstageService _vvmstageService;

        public VvmstageController(IVvmstageService vvmstageService)
        {
            _vvmstageService = vvmstageService;
        }

        // -------------------- Index / List --------------------
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

            return View(vvmstages); // View file: /Views/Vvmstage/Index.cshtml
        }

        // -------------------- New (GET) --------------------
        public IActionResult New()
        {
            return View(new VvmstageViewModel()); // View file: /Views/Vvmstage/New.cshtml
        }

        // -------------------- Edit (GET) --------------------
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

            return View("New", model); // Reuse the New view for editing
        }

        // -------------------- Create / Update (POST) --------------------
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
                EntryDate = model.EntryDate,
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

        // -------------------- Delete (POST) --------------------
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