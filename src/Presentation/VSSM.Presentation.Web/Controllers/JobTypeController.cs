using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.JobType;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.JobType;

namespace VSSM.Presentation.Web.Controllers
{
    
    public class JobTypeController : BaseController
    {
        private readonly IJobTypeService _service;

        public JobTypeController(IJobTypeService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index(
            CancellationToken token,
            string? search,
            int page = 1,
            int pageSize = 10,
            string sortField = "FullName",
            string sortOrder = "asc")
        {
            var listModel = await _service.GetPagedList(
                search, page, pageSize, sortField, sortOrder, token);

            return View(listModel);
        }

        public async Task<IActionResult> New(int? id, CancellationToken token)
        {
            JobTypeViewModel model = new JobTypeViewModel();

            if (id.HasValue && id.Value > 0)
            {
                var dto = await _service.GetById(id.Value, token);

                model = new JobTypeViewModel
                {
                    JobTypeId = dto.JobTypeId,
                    FullName = dto.FullName,
                    IsActive = dto.IsActive
                };
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(JobTypeViewModel model, CancellationToken token)
        {
            if (!ModelState.IsValid)
                return View("New", model);

            if (model.JobTypeId == null || model.JobTypeId == 0)
            {
                await _service.Create(new JobTypeCreateDto
                {
                    FullName = model.FullName,
                    IsActive = model.IsActive
                }, token);
            }
            else
            {
                await _service.Update(new JobTypeEditDto
                {
                    JobTypeId = model.JobTypeId.Value,
                    FullName = model.FullName,
                    IsActive = model.IsActive
                }, token);
            }

            TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
            TempData["ToastType"] = "success";

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id, CancellationToken token)
        {
            var dto = await _service.GetById(id, token);

            var model = new JobTypeViewModel
            {
                JobTypeId = dto.JobTypeId,
                FullName = dto.FullName,
                IsActive = dto.IsActive
            };

            return View("New", model);
        }
    }
}
