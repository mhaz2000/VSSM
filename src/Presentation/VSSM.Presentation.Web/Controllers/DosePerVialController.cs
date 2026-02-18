using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.DosePerVial;
using VSSM.Application.DTOs.BaseEntities.Item;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Application.Services.BaseEntities;
using VSSM.Presentation.Web.Models.DosePerVial;
using VSSM.Presentation.Web.Models.Item;

namespace VSSM.Presentation.Web.Controllers
{
    
    public class DosePerVialController : BaseController
    {
        private readonly IDosePerVialService _dosePerVialService;
        public DosePerVialController(IDosePerVialService dosePerVialService)
        {
            _dosePerVialService = dosePerVialService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken, string? search,
                                                int page = 1,
                                                int pageSize = 10,
                                                string sortField = "Orders",
                                                string sortOrder = "desc")
        {
            var listModel = await _dosePerVialService.GetPagedList(search,
                                                                    page,
                                                                    pageSize,
                                                                    sortField,
                                                                    sortOrder,
                                                                    cancellationToken);

            return View(listModel);

        }

        public async Task<IActionResult> New()
        {
            return View();
        }

        public async Task<IActionResult> Create(DosePerVialViewModel model, CancellationToken cancellationTOken)
        {
            if (!ModelState.IsValid)
            {
                return Redirect("New");
            }

            var dto = new DosePerVialCreateDto
            {
                DosePerVialId = model.DosePerVialId.GetValueOrDefault(),
                NumDosePerVial = model.NumDosePerVial,
                IsActive = model.IsActive,
            };

            if (model.DosePerVialId == null || model.DosePerVialId == 0)
            {
                // ---------- Create ----------
                var res = await _dosePerVialService.Create(dto, cancellationTOken);
            }
            else
            {
                // ---------- Update ----------
                var res = await _dosePerVialService.Update(dto, cancellationTOken);
            }

            TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
            TempData["ToastType"] = "success";

            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            // داده را از دیتابیس می‌خوانیم
            var dose = await _dosePerVialService.GetById(id, cancellationToken);
            if (dose == null)
                return NotFound();

            var model = new DosePerVialViewModel
            {
                DosePerVialId = dose.DosePerVialId,
                NumDosePerVial = dose.NumDosePerVial,
                IsActive = dose.IsActive.GetValueOrDefault()
            };

            // نمایش فرم ویرایش با داده‌ها
            return View("New", model);
        }



    }
}
