
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VSSM.Application.DTOs.BaseEntities.Store;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.Store;

namespace VSSM.Presentation.Web.Controllers
{
    public class StoreController : Controller
    {
        private readonly IStoreService _storeService;
        private readonly IStoreTypeService _storeTypeService;
        private readonly IRecipientService _recipientService;
        private readonly IPowerSourceService _powerSourceService;
        private readonly ISourceService _sourceService;

        public StoreController(
            IStoreService storeService,
            IStoreTypeService storeTypeService,
            IRecipientService recipientService,
            IPowerSourceService powerSourceService,
            ISourceService sourceService)
        {
            _storeService = storeService;
            _storeTypeService = storeTypeService;
            _recipientService = recipientService;
            _powerSourceService = powerSourceService;
            _sourceService = sourceService;
        }

        public async Task<IActionResult> Index(
         string? search,
         int? typeId,
         int page = 1,
         int pageSize = 10,
         CancellationToken token = default)
        {
            ViewBag.TypeList = (await _storeTypeService.GetAll(token))
                .Select(x => new SelectListItem
                {
                    Value = x.StoreTypeId.ToString(),
                    Text = x.FullName
                })
                .ToList();

            var list = await _storeService.GetPagedList(search, typeId, page, pageSize, token);

            return View(list);
        }

        public async Task<IActionResult> New(int? id, CancellationToken cancellationToken)
        {
            await LoadDropDowns(cancellationToken);

            StoreViewModel models = new StoreViewModel
            {
                IsActive = true
            };

            if (id.HasValue && id.Value > 0)
            {
                var dto = await _storeService.GetById(id.Value, cancellationToken);

                models = new StoreViewModel
                {
                    StoreId = dto.StoreId,
                    Code = dto.Code,
                    Name = dto.Name,
                    TypeId = dto.TypeId,
                    FreeVolume = dto.FreeVolume,
                    Volume = dto.Volume,
                    Note = dto.Note,
                    IsActive = dto.IsActive,
                    RecipientId = dto.RecipientId,

                    Year = dto.Year,
                    Model = dto.Model,
                    Manufacturer = dto.Manufacturer,
                    Pqsno = dto.Pqsno,
                    CoolingUnitNo = dto.CoolingUnitNo,
                    RefrigerantGas = dto.RefrigerantGas,

                    PowerSource = dto.PowerSource,
                    Source = dto.Source,
                    Barcode = dto.Barcode,
                    PeriodNoteUse = dto.PeriodNoteUse,
                    UnitCost = dto.UnitCost
                };
            }

            return View(models);
        }

        [HttpPost]
        public async Task<IActionResult> Create(StoreViewModel storeView, CancellationToken cancellationToken)
        {
            await LoadDropDowns(cancellationToken);
            if (!ModelState.IsValid)
            {

                return View("New", storeView);
            }

            if (storeView.StoreId == null || storeView.StoreId == 0)
            {
                var dto = new StoreCreateDto
                {
                    Code = storeView.Code,
                    Name = storeView.Name,
                    TypeId = storeView.TypeId.Value,
                    FreeVolume = storeView.FreeVolume.Value,
                    Volume = storeView.Volume,
                    Note = storeView.Note,
                    IsActive = storeView.IsActive,
                    RecipientId = storeView.RecipientId,

                    Year = storeView.Year,
                    Model = storeView.Model,
                    Manufacturer = storeView.Manufacturer,
                    Pqsno = storeView.Pqsno,
                    CoolingUnitNo = storeView.CoolingUnitNo,
                    RefrigerantGas = storeView.RefrigerantGas,

                    PowerSource = storeView.PowerSource,
                    Source = storeView.Source,
                    Barcode = storeView.Barcode,
                    PeriodNoteUse = storeView.PeriodNoteUse,
                    UnitCost = storeView.UnitCost
                };

                await _storeService.Create(dto, cancellationToken);
            }
            else
            {
                var dto = new StoreEditDto
                {
                    StoreId = storeView.StoreId.Value,
                    Code = storeView.Code,
                    Name = storeView.Name,
                    TypeId = storeView.TypeId.Value,
                    FreeVolume = storeView.FreeVolume.Value,
                    Volume = storeView.Volume,
                    Note = storeView.Note,
                    IsActive = storeView.IsActive,
                    RecipientId = storeView.RecipientId,

                    Year = storeView.Year,
                    Model = storeView.Model,
                    Manufacturer = storeView.Manufacturer,
                    Pqsno = storeView.Pqsno,
                    CoolingUnitNo = storeView.CoolingUnitNo,

                    PowerSource = storeView.PowerSource,
                    Source = storeView.Source,
                    Barcode = storeView.Barcode,
                    PeriodNoteUse = storeView.PeriodNoteUse,
                    UnitCost = storeView.UnitCost
                };

                await _storeService.Update(dto, cancellationToken);
            }

            TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
            TempData["ToastType"] = "success";

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            return await New(id, cancellationToken);
        }

        private async Task LoadDropDowns(CancellationToken cancellationToken)
        {
            var storeTypes = await _storeTypeService.GetAll(cancellationToken);
            var recipients = await _recipientService.GetAll(cancellationToken);
            var powerSources = await _powerSourceService.GetAll(cancellationToken);
            var sources = await _sourceService.GetAll(cancellationToken);

            ViewBag.StoreTypeList = storeTypes
                .Select(x => new SelectListItem
                {
                    Text = x.FullName,
                    Value = x.StoreTypeId.ToString()
                })
                .ToList();

            ViewBag.RecipientList = recipients
                .Select(x => new SelectListItem
                {
                    Text = x.FullName,
                    Value = x.RecipientId.ToString()
                })
                .ToList();

            ViewBag.PowerSourceList = powerSources
                .Select(x => new SelectListItem
                {
                    Text = x.FullName,
                    Value = x.PowerSourceId.ToString()
                })
                .ToList();

            ViewBag.SourceList = sources
                .Select(x => new SelectListItem
                {
                    Text = x.FullName,
                    Value = x.SourceId.ToString()
                })
                .ToList();
        }
    }
}
