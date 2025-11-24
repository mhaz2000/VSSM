using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.RecipientMaxItem;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.RecipientMaxItem;

namespace VSSM.Presentation.Web.Controllers;

public class RecipientMaxItemController : Controller
    {
        private readonly IRecipientMaxItemService _recipientMaxItemService;

        public RecipientMaxItemController(IRecipientMaxItemService recipientMaxItemService)
        {
            _recipientMaxItemService = recipientMaxItemService;
        }

        // -------------------- Index / List --------------------
        public async Task<IActionResult> Index(
            CancellationToken cancellationToken,
            string? search = null,
            int page = 1,
            int pageSize = 10,
            string sortField = "RecipientMaxItemId",
            string sortOrder = "asc")
        {
            var items = await _recipientMaxItemService.GetPagedList(
                search,
                page,
                pageSize,
                sortField,
                sortOrder,
                cancellationToken);

            return View(items); // View file: /Views/RecipientMaxItem/Index.cshtml
        }

        // -------------------- New (GET) --------------------
        public IActionResult New()
        {
            return View(new RecipientMaxItemViewModel()); // View file: /Views/RecipientMaxItem/New.cshtml
        }

        // -------------------- Edit (GET) --------------------
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var item = await _recipientMaxItemService.GetById(id, cancellationToken);
            if (item == null)
                return NotFound();

            var model = new RecipientMaxItemViewModel
            {
                RecipientMaxItemId = item.RecipientMaxItemId,
                ItemId = item.ItemId,
                RecipientId = item.RecipientId,
                NumberDose = item.NumberDose,
                ReceiverId = item.ReceiverId,
                RelatedToItem = item.RelatedToItem
            };

            return View("New", model); // Reuse the New view for editing
        }

        // -------------------- Create / Update (POST) --------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RecipientMaxItemViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View("New", model);
            }

            var dto = new RecipientMaxItemDto
            {
                RecipientMaxItemId = model.RecipientMaxItemId.GetValueOrDefault(),
                ItemId = model.ItemId,
                RecipientId = model.RecipientId,
                NumberDose = model.NumberDose,
                ReceiverId = model.ReceiverId,
                RelatedToItem = model.RelatedToItem
            };

            if (model.RecipientMaxItemId == null || model.RecipientMaxItemId == 0)
            {
                await _recipientMaxItemService.Create(dto, cancellationToken);
            }
            else
            {
                await _recipientMaxItemService.Update(dto, cancellationToken);
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
            var deleted = await _recipientMaxItemService.Delete(id, cancellationToken);

            if (deleted)
            {
                TempData["ToastMessage"] = "عملیات حذف با موفقیت انجام شد";
                TempData["ToastType"] = "success";
            }
            else
            {
                TempData["ToastMessage"] = "آیتم مورد نظر یافت نشد";
                TempData["ToastType"] = "error";
            }

            return RedirectToAction("Index");
        }
    }