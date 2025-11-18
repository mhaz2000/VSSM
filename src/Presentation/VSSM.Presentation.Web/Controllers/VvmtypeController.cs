using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.Vvmtype;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.Vvmtype;

namespace VSSM.Presentation.Web.Controllers;

public class VvmtypeController : Controller
    {
        private readonly IVvmtypeService _vvmtypeService;

        public VvmtypeController(IVvmtypeService vvmtypeService)
        {
            _vvmtypeService = vvmtypeService;
        }

        // -------------------- Index / List --------------------
        public async Task<IActionResult> Index(
            CancellationToken cancellationToken,
            string? search = null,
            int page = 1,
            int pageSize = 10,
            string sortField = "VvmtypeId",
            string sortOrder = "asc")
        {
            var vvmtypes = await _vvmtypeService.GetPagedList(
                search,
                page,
                pageSize,
                sortField,
                sortOrder,
                cancellationToken);

            return View(vvmtypes); // View file: /Views/Vvmtype/Index.cshtml
        }

        // -------------------- New (GET) --------------------
        public IActionResult New()
        {
            return View(new VvmtypeViewModel()); // View file: /Views/Vvmtype/New.cshtml
        }

        // -------------------- Edit (GET) --------------------
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var vvmtype = await _vvmtypeService.GetById(id, cancellationToken);
            if (vvmtype == null)
                return NotFound();

            var model = new VvmtypeViewModel
            {
                VvmtypeId = vvmtype.VvmtypeId,
                Code = vvmtype.Code,
                FullName = vvmtype.FullName,
                CanUserChange = vvmtype.CanUserChange,
                EntryDate = vvmtype.EntryDate,
                IsActive = vvmtype.IsActive
            };

            return View("New", model); // Reuse the New view for editing
        }

        // -------------------- Create / Update (POST) --------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VvmtypeViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View("New", model);
            }

            var dto = new VvmtypeDto
            {
                VvmtypeId = model.VvmtypeId.GetValueOrDefault(),
                Code = model.Code,
                FullName = model.FullName,
                CanUserChange = model.CanUserChange,
                EntryDate = model.EntryDate,
                IsActive = model.IsActive
            };

            if (model.VvmtypeId == null || model.VvmtypeId == 0)
            {
                await _vvmtypeService.Create(dto, cancellationToken);
            }
            else
            {
                await _vvmtypeService.Update(dto, cancellationToken);
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
            var deleted = await _vvmtypeService.Delete(id, cancellationToken);

            if (deleted)
            {
                TempData["ToastMessage"] = "عملیات حذف با موفقیت انجام شد";
                TempData["ToastType"] = "success";
            }
            else
            {
                TempData["ToastMessage"] = "نوع مورد نظر یافت نشد";
                TempData["ToastType"] = "error";
            }

            return RedirectToAction("Index");
        }
    }