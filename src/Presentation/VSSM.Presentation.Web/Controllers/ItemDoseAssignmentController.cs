using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.Item;
using VSSM.Application.DTOs.BaseEntities.ItemDoseAssignment;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Application.Services.BaseEntities;
using VSSM.Presentation.Web.Models.Item;
using VSSM.Presentation.Web.Models.ItemDoseAssignment;

namespace VSSM.Presentation.Web.Controllers
{
    [Authorize]
    public class ItemDoseAssignmentController : BaseController
    {
        private readonly IItemService _itemService;
        private readonly IItemClassService _itemClassService;
        private readonly IItemDoseAssignmentService _itemDoseAssignmentService;
        private readonly IDosePerVialService _dosePerVialService;
        public ItemDoseAssignmentController(IItemDoseAssignmentService itemDoseAssignmentService,
                                            IDosePerVialService dosePerVialService,
                                            IItemService itemService, IItemClassService itemClassService)
        {
            _itemService = itemService;
            _itemClassService = itemClassService;
            _itemDoseAssignmentService = itemDoseAssignmentService;
            _dosePerVialService = dosePerVialService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken, string? search,
                                                int page = 1,
                                                int pageSize = 10,
                                                string sortField = "Orders",
                                                string sortOrder = "desc")
        {
            var listModel = await _itemDoseAssignmentService.GetPagedList(search,
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

        public async Task<IActionResult> Create(ItemDoseAssignmentViewModel model, CancellationToken cancellationTOken)
        {
            if (!ModelState.IsValid)
            {
                return Redirect("New");
            }

            var dto = new ItemDoseAssignmentCreateDto
            {
                ItemDoseAssignmentId = model.ItemDoseAssignmentId.GetValueOrDefault(),
                ItemId = model.ItemId,
                DoseId = model.DoseId,
                VialPerDoseCm3 = model.VialPerDoseCm3
            };

            if (model.ItemDoseAssignmentId == null || model.ItemDoseAssignmentId == 0)
            {
                // ---------- Create ----------
                var res = await _itemDoseAssignmentService.Create(dto, cancellationTOken);
            }
            else
            {
                // ---------- Update ----------
                var res = await _itemDoseAssignmentService.Update(dto, cancellationTOken);
            }

            TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
            TempData["ToastType"] = "success";

            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            // داده را از دیتابیس می‌خوانیم
            var item = await _itemDoseAssignmentService.GetById(id, cancellationToken);
            if (item == null)
                return NotFound();

            var model = new ItemDoseAssignmentViewModel
            {
                ItemDoseAssignmentId = item.ItemDoseAssignmentId,
                ItemId = item.ItemId.GetValueOrDefault(),
                DoseId = item.DoseId.GetValueOrDefault(),
                VialPerDoseCm3 = item.VialPerDoseCm3.GetValueOrDefault()
            };

            await PrepareDropdown(cancellationToken);
            // نمایش فرم ویرایش با داده‌ها
            return View("New", model);
        }

        private async Task PrepareDropdown(CancellationToken cancellationTOken)
        {
            var itemclass = await _itemClassService.GetActives(cancellationTOken);
            ViewBag.ItemClass = itemclass;

            var item = await _itemService.GetActives(cancellationTOken);
            ViewBag.Item = item;

            var dosePerVial = await _dosePerVialService.GetActives(cancellationTOken);
            ViewBag.DosePerVial = dosePerVial;
        }

    }
}
