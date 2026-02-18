using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.EduType;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.EduType;

namespace VSSM.Presentation.Web.Controllers
{
    
    public class EduTypeController : BaseController
    {
        private readonly IEduTypeService _service;

        public EduTypeController(IEduTypeService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index(
            CancellationToken cancellationToken,
            string? search,
            int page = 1,
            int pageSize = 10,
            string sortField = "FullName",
            string sortOrder = "asc")
        {
            var listModel = await _service.GetPagedList(
                search, page, pageSize, sortField, sortOrder, cancellationToken);

            return View(listModel);
        }

        public async Task<IActionResult> New(int? id, CancellationToken cancellationToken)
        {
            EduTypeViewModel model = new EduTypeViewModel();

            if (id.HasValue && id.Value > 0)
            {
                var dto = await _service.GetById(id.Value, cancellationToken);
                model = new EduTypeViewModel
                {
                    EduTypeId = dto.EduTypeId,
                    FullName = dto.FullName,
                    IsActive = dto.IsActive
                };
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(EduTypeViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View("New", model);

            if (model.EduTypeId == null || model.EduTypeId == 0)
            {
                var dto = new EduTypeCreateDto
                {
                    FullName = model.FullName,
                    IsActive = model.IsActive
                };
                await _service.Create(dto, cancellationToken);
            }
            else
            {
                var dto = new EduTypeEditDto
                {
                    EduTypeId = model.EduTypeId.Value,
                    FullName = model.FullName,
                    IsActive = model.IsActive
                };
                await _service.Update(dto, cancellationToken);
            }

            TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
            TempData["ToastType"] = "success";

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var dto = await _service.GetById(id, cancellationToken);

            var model = new EduTypeViewModel
            {
                EduTypeId = dto.EduTypeId,
                FullName = dto.FullName,
                IsActive = dto.IsActive
            };

            return View("New", model);
        }
    }
}
