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
            CancellationToken cancellationToken,
            string? search,
            int page = 1,
            int pageSize = 10,
            string sortField = "Code",
            string sortOrder = "asc")
        {
            var listModel = await _storeService.GetPagedList(
                search, page, pageSize, sortField, sortOrder, cancellationToken);

            return View(listModel);
        }

        public async Task<IActionResult> New(int? id, CancellationToken cancellationToken)
        {
            await LoadDropDowns(cancellationToken);

            StoreViewModel model = new StoreViewModel
            {
                IsActive = true
            };

            if (id.HasValue && id.Value > 0)
            {
                var dto = await _storeService.GetById(id.Value, cancellationToken);

                model = new StoreViewModel
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
                    GrossVol = dto.GrossVol,
                    Year = dto.Year,
                    Model = dto.Model,
                    Manufacturer = dto.Manufacturer,
                    Pqsno = dto.Pqsno,
                    CoolingUnitNo = dto.CoolingUnitNo,
                    RefrigerantGas = dto.RefrigerantGas,
                    RoomBox = dto.RoomBox,
                    PoweSource = dto.PoweSource,
                    Source = dto.Source,
                    Barcode = dto.Barcode,
                    PeriodNotUse = dto.PeriodNotUse,
                    UnitCost = dto.UnitCost
                };
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(StoreViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropDowns(cancellationToken);
                return View("New", model);
            }

            if (model.StoreId == null || model.StoreId == 0)
            {
                var dto = new StoreCreateDto
                {
                    Code = model.Code,
                    Name = model.Name,
                    TypeId = model.TypeId.Value,
                    FreeVolume = model.FreeVolume.Value,
                    Volume = model.Volume,
                    Note = model.Note,
                    IsActive = model.IsActive,
                    RecipientId = model.RecipientId,
                    GrossVol = model.GrossVol,
                    Year = model.Year,
                    Model = model.Model,
                    Manufacturer = model.Manufacturer,
                    Pqsno = model.Pqsno,
                    CoolingUnitNo = model.CoolingUnitNo,
                    RefrigerantGas = model.RefrigerantGas,
                    RoomBox = model.RoomBox,
                    PoweSource = model.PoweSource,
                    Source = model.Source,
                    Barcode = model.Barcode,
                    PeriodNotUse = model.PeriodNotUse,
                    UnitCost = model.UnitCost
                };

                await _storeService.Create(dto, cancellationToken);
            }
            else
            {
                var dto = new StoreEditDto
                {
                    StoreId = model.StoreId.Value,
                    Code = model.Code,
                    Name = model.Name,
                    TypeId = model.TypeId.Value,
                    FreeVolume = model.FreeVolume.Value,
                    Volume = model.Volume,
                    Note = model.Note,
                    IsActive = model.IsActive,
                    RecipientId = model.RecipientId,
                    GrossVol = model.GrossVol,
                    Year = model.Year,
                    Model = model.Model,
                    Manufacturer = model.Manufacturer,
                    Pqsno = model.Pqsno,
                    CoolingUnitNo = model.CoolingUnitNo,
                    RefrigerantGas = model.RefrigerantGas,
                    RoomBox = model.RoomBox,
                    PoweSource = model.PoweSource,
                    Source = model.Source,
                    Barcode = model.Barcode,
                    PeriodNotUse = model.PeriodNotUse,
                    UnitCost = model.UnitCost
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
