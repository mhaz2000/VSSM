using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.RoadType;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.RoadType;

namespace VSSM.Presentation.Web.Controllers
{
    public class RoadTypeController : Controller
    {
        private readonly IRoadTypeService _roadTypeService;

        public RoadTypeController(IRoadTypeService roadTypeService)
        {
            _roadTypeService = roadTypeService;
        }

        // لیست + جستجو + صفحه‌بندی + مرتب‌سازی
        public async Task<IActionResult> Index(
            CancellationToken cancellationToken,
            string? search,
            int page = 1,
            int pageSize = 10,
            string sortField = "Code",
            string sortOrder = "asc")
        {
            var listModel = await _roadTypeService.GetPagedList(
                search,
                page,
                pageSize,
                sortField,
                sortOrder,
                cancellationToken);

            return View(listModel);
        }

        // نمایش فرم ثبت/ویرایش
        public async Task<IActionResult> New(int? id, CancellationToken cancellationToken)
        {
            RoadTypeViewModel model = new RoadTypeViewModel();

            if (id.HasValue && id.Value > 0)
            {
                var dto = await _roadTypeService.GetById(id.Value, cancellationToken);
                if (dto == null)
                    return NotFound();

                model = new RoadTypeViewModel
                {
                    RoadTypeId = dto.RoadTypeId,
                    Code = dto.Code,
                    FullName = dto.FullName,
                    IsActive = dto.IsActive
                };
            }

            return View(model);
        }

        // Create + Update (مثل ItemClass)
        [HttpPost]
        public async Task<IActionResult> Create(RoadTypeViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View("New", model);
            }

            if (model.RoadTypeId == null || model.RoadTypeId == 0)
            {
                // ---------- Create ----------
                var dto = new RoadTypeCreateDto
                {
                    Code = model.Code,
                    FullName = model.FullName,
                    IsActive = model.IsActive
                };

                var res = await _roadTypeService.Create(dto, cancellationToken);
            }
            else
            {
                // ---------- Update ----------
                var dto = new RoadTypeEditDto
                {
                    RoadTypeId = model.RoadTypeId.Value,
                    Code = model.Code,
                    FullName = model.FullName,
                    IsActive = model.IsActive
                };

                var res = await _roadTypeService.Update(dto, cancellationToken);
            }

            TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
            TempData["ToastType"] = "success";

            return RedirectToAction("Index");
        }

        // ویرایش مستقیم مانند ItemClass/Edit (اختیاری، ولی کاملش می‌کنیم)
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var dto = await _roadTypeService.GetById(id, cancellationToken);
            if (dto == null)
                return NotFound();

            var model = new RoadTypeViewModel
            {
                RoadTypeId = dto.RoadTypeId,
                Code = dto.Code,
                FullName = dto.FullName,
                IsActive = dto.IsActive
            };

            // استفاده از همان فرم New برای Edit
            return View("New", model);
        }
    }
}
