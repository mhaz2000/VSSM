using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VSSM.Application.DTOs.BaseEntities.RecipientConfiguration;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.RecipientConfiguration;

namespace VSSM.Presentation.Web.Controllers
{
    [Authorize]
    public class RecipientConfigurationController : BaseController
    {
        private readonly IRecipientConfigurationService _service;
        private readonly IRecipientService _recipientService;

        public RecipientConfigurationController(
            IRecipientConfigurationService service,
            IRecipientService recipientService)
        {
            _service = service;
            _recipientService = recipientService;
        }

        public async Task<IActionResult> Index(CancellationToken token)
        {
            var dto = await _service.GetSingle(token);

            if (dto == null)
            {
                // Show empty index with Create button
                return View(null);
            }

            var vm = new RecipientConfigurationViewModel
            {
                RecipientConfigurationId = dto.RecipientConfigurationId,
                UseBundles = dto.UseBundles ?? false,
                FreezeQtyDispatchedDl = dto.FreezeQtyDispatchedDl ?? false,
                CurrentLang = dto.CurrentLang,
                CurrentVoucherNumber = dto.CurrentVoucherNumber,
                RecipientId = dto.RecipientId
            };

            return View(vm);
        }

        public async Task<IActionResult> New(CancellationToken token)
        {
            await LoadRecipients(token);

            return View(new RecipientConfigurationViewModel());
        }

        public async Task<IActionResult> Edit(int id, CancellationToken token)
        {
            var dto = await _service.GetById(id, token);
            await LoadRecipients(token);

            if (dto == null)
                return RedirectToAction("Index");

            var vm = new RecipientConfigurationViewModel
            {
                RecipientConfigurationId = dto.RecipientConfigurationId,
                UseBundles = dto.UseBundles ?? false,
                FreezeQtyDispatchedDl = dto.FreezeQtyDispatchedDl ?? false,
                CurrentLang = dto.CurrentLang,
                CurrentVoucherNumber = dto.CurrentVoucherNumber,
                RecipientId = dto.RecipientId
            };

            return View("New", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(RecipientConfigurationViewModel model, CancellationToken token)
        {
            await LoadRecipients(token);

            if (!ModelState.IsValid)
                return View("New", model);

            if (model.RecipientConfigurationId == null)
            {
                var dto = new RecipientConfigurationCreateDto
                {
                    UseBundles = model.UseBundles,
                    FreezeQtyDispatchedDl = model.FreezeQtyDispatchedDl,
                    CurrentLang = model.CurrentLang,
                    CurrentVoucherNumber = model.CurrentVoucherNumber,
                    RecipientId = model.RecipientId,
                    EntryDate = DateTime.Now,
                    LastLogin = DateTime.Now
                };

                await _service.Create(dto, token);
            }
            else
            {
                var dto = new RecipientConfigurationEditDto
                {
                    RecipientConfigurationId = model.RecipientConfigurationId.Value,
                    UseBundles = model.UseBundles,
                    FreezeQtyDispatchedDl = model.FreezeQtyDispatchedDl,
                    CurrentLang = model.CurrentLang,
                    CurrentVoucherNumber = model.CurrentVoucherNumber,
                    RecipientId = model.RecipientId,
                    EntryDate = DateTime.Now,
                    LastLogin = DateTime.Now
                };

                await _service.Update(dto, token);
            }

            TempData["ToastMessage"] = "با موفقیت ذخیره شد";
            TempData["ToastType"] = "success";

            return RedirectToAction("Index");
        }

        private async Task LoadRecipients(CancellationToken token)
        {
            var recipients = await _recipientService.GetAll(token);

            ViewBag.RecipientList = recipients
                .Select(x => new SelectListItem
                {
                    Text = x.FullName,
                    Value = x.RecipientId.ToString()
                })
                .ToList();

            ViewBag.LanguageList = new List<SelectListItem>
            {
                new SelectListItem {Text="فارسی", Value="fa"},
                new SelectListItem {Text="English", Value="en"}
            };
        }
    }
}
