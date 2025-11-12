using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.Levels;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.Level;

namespace VSSM.Presentation.Web.Controllers
{
    public class LevelController : Controller
    {
        private readonly ILevelService _levelService;

        public LevelController(ILevelService levelService)
        {
            _levelService = levelService;
        }

        // -------------------- Index / List --------------------
        public async Task<IActionResult> Index(
            CancellationToken cancellationToken,
            string? search = null,
            int page = 1,
            int pageSize = 10,
            string sortField = "LevelId",
            string sortOrder = "asc")
        {
            var levels = await _levelService.GetPagedList(
                search,
                page,
                pageSize,
                sortField,
                sortOrder,
                cancellationToken);

            return View(levels); // View file: /Views/Level/Index.cshtml
        }

        // -------------------- New (GET) --------------------
        public IActionResult New()
        {
            return View(new LevelViewModel()); // View file: /Views/Level/New.cshtml
        }

        // -------------------- Edit (GET) --------------------
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var level = await _levelService.GetById(id, cancellationToken);
            if (level == null)
                return NotFound();

            var model = new LevelViewModel
            {
                LevelId = level.LevelId,
                ShipCount = level.ShipCount,
                SafetyStockPercentage = level.SafetyStockPercentage,
                SafetyStockMonth = level.SafetyStockMonth,
                Note = level.Note,
                MaxDay = level.MaxDay
            };

            return View("New", model); // Reuse the New view for editing
        }

        // -------------------- Create / Update (POST) --------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LevelViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View("New", model);
            }

            var dto = new LevelDto
            {
                LevelId = model.LevelId.GetValueOrDefault(),
                ShipCount = model.ShipCount,
                SafetyStockPercentage = model.SafetyStockPercentage,
                SafetyStockMonth = model.SafetyStockMonth,
                Note = model.Note,
                MaxDay = model.MaxDay
            };

            if (model.LevelId == null || model.LevelId == 0)
            {
                await _levelService.Create(dto, cancellationToken);
            }
            else
            {
                await _levelService.Update(dto, cancellationToken);
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
            var deleted = await _levelService.Delete(id, cancellationToken);

            if (deleted)
            {
                TempData["ToastMessage"] = "عملیات حذف با موفقیت انجام شد";
                TempData["ToastType"] = "success";
            }
            else
            {
                TempData["ToastMessage"] = "سطح مورد نظر یافت نشد";
                TempData["ToastType"] = "error";
            }

            return RedirectToAction("Index");
        }
    }
}
