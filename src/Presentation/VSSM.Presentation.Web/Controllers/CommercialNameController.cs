using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.CommercialName;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.CommercialName;

namespace VSSM.Presentation.Web.Controllers
{
    [Authorize]
    public class CommercialNameController : BaseController
    {
        private readonly ICommercialNameService _commercialNameService;

        public CommercialNameController(ICommercialNameService commercialNameService)
        {
            _commercialNameService = commercialNameService;
        }

        public async Task<IActionResult> Index(
            CancellationToken cancellationToken,
            string? search = null,
            int page = 1,
            int pageSize = 10,
            string sortField = "CommercialNameId",
            string sortOrder = "asc")
        {
            var commercialNames = await _commercialNameService.GetPagedList(
                search,
                page,
                pageSize,
                sortField,
                sortOrder,
                cancellationToken);

            return View(commercialNames); 
        }

        public IActionResult New()
        {
            return View(new CommercialNameViewModel()); 
        }

        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var commercialName = await _commercialNameService.GetById(id, cancellationToken);
            if (commercialName == null)
                return NotFound();

            var model = new CommercialNameViewModel
            {
                CommercialNameId = commercialName.CommercialNameId,
                FullName = commercialName.FullName,
                Code = commercialName.Code,
                IsActive = commercialName.IsActive
            };

            return View("New", model); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CommercialNameViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View("New", model);
            }

            var dto = new CommercialNameDto
            {
                CommercialNameId = model.CommercialNameId.GetValueOrDefault(),
                FullName = model.FullName,
                Code = model.Code,
                IsActive = model.IsActive,
                EntryDate = DateTime.Now
            };

            if (model.CommercialNameId == null || model.CommercialNameId == 0)
            {
                await _commercialNameService.Create(dto, cancellationToken);
            }
            else
            {
                await _commercialNameService.Update(dto, cancellationToken);
            }

            TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
            TempData["ToastType"] = "success";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var deleted = await _commercialNameService.Delete(id, cancellationToken);

            if (deleted)
            {
                TempData["ToastMessage"] = "عملیات حذف با موفقیت انجام شد";
                TempData["ToastType"] = "success";
            }
            else
            {
                TempData["ToastMessage"] = "نام تجاری مورد نظر یافت نشد";
                TempData["ToastType"] = "error";
            }

            return RedirectToAction("Index");
        }
    }
}
