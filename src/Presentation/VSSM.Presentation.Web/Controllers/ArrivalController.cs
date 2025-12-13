using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VSSM.Application.DTOs.BaseEntities.Arrival;
using VSSM.Application.Helpers;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.Arrival;

namespace VSSM.Presentation.Web.Controllers
{
    public class ArrivalController : Controller
    {
        private readonly IArrivalService _arrivalService;
        private readonly ISourceService _sourceService;
        private readonly IFiscalPeriodService _fiscalService;
        private readonly IItemService _itemService;
        private readonly IStoreService _storeService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IDosePerVialService _dosePerVialService;
        private readonly IVvmtypeService _vvmtypeService;
        private readonly IVvmstageService _vvmstageService;
        private readonly ICommercialNameService _commercialNameService;
        private readonly IItemDetailsService _itemDetailService;
        private readonly IUseService _useService;
        private readonly IPackingTypeService _packingTypeService;

        public ArrivalController(
            IArrivalService arrivalService,
            ISourceService sourceService,
            IFiscalPeriodService fiscalService,
            IItemService itemService,
            IStoreService storeService,
            IManufacturerService manufacturerService,
            IDosePerVialService dosePerVialService,
            IVvmtypeService vvmtypeService,
            IVvmstageService vvmstageService,
            ICommercialNameService commercialNameService,
            IItemDetailsService itemDetailService,
            IUseService useService,
            IPackingTypeService packingTypeService)
        {
            _arrivalService = arrivalService;
            _sourceService = sourceService;
            _fiscalService = fiscalService;
            _itemService = itemService;
            _storeService = storeService;
            _manufacturerService = manufacturerService;
            _dosePerVialService = dosePerVialService;
            _vvmtypeService = vvmtypeService;
            _vvmstageService = vvmstageService;
            _commercialNameService = commercialNameService;
            _itemDetailService = itemDetailService;
            _useService = useService;
            _packingTypeService = packingTypeService;
        }

        public async Task<IActionResult> Index(
            string? search,
            int status = 1, // 1 Draft, 2 Confirmed, 3 All
            int page = 1,
            int pageSize = 10,
            CancellationToken token = default)
        {
            ViewBag.Status = status;


            var list = await _arrivalService.GetPagedList(
                search,
                status,
                page,
                pageSize,
                token);

            return View("Index", list);
        }

        public async Task<IActionResult> New(int? id, CancellationToken token)
        {
            await LoadDropDowns(token);

            // Create mode
            if (id == null || id == 0)
            {
                var vm = new ArrivalViewModel
                {
                    Date = DateTime.Now.ToString("yyyy/MM/dd"),
                    IsDrafted = true,
                    Lines = new List<ArrivalLineViewModel>(),
                    NewLine = new ArrivalLineViewModel()
                };
                return View(vm);
            }

            // Edit mode
            var dto = await _arrivalService.GetById(id.Value, token);
            if (dto == null) return NotFound();

            var model = new ArrivalViewModel
            {
                ArrivalId = dto.ArrivalId,
                Date = dto.Date.HasValue ? DateTimeHelper.ToShamsi(dto.Date.Value) : "",
                ReportNumber = dto.ReportNumber,
                SourceId = dto.SourceId,
                FiscalPeriodId = dto.FiscalPeriodId,
                EntryDate = dto.EntryDate,
                IsConfirmed = dto.IsConfirmed ?? false,
                IsDrafted = dto.IsDrafted ?? false,
                IsCancelled = dto.IsCancelled ?? false,
                PackedVolume = dto.PackedVolume,
                TotalWeight = dto.TotalWeight,
                TotalVolume = dto.TotalVolume,
                Note = dto.Note,
                Frzindc = dto.Frzindc,
                FrzindcNok = dto.FrzindcNok,
                Lines = dto.Lines.Select(l => new ArrivalLineViewModel
                {
                    ArrivalLineId = l.ArrivalLineId,
                    ArrivalId = l.ArrivalId,

                    BatchNum = l.BatchNum,
                    CommertialNameId = l.CommertialNameId,
                    DosePerVialId = l.DosePerVialId,

                    ExpiryDate = l.ExpiryDate.HasValue ? DateTimeHelper.ToShamsi(l.ExpiryDate.Value) : "",
                    ProductionDate = l.ProductionDate.HasValue ? DateTimeHelper.ToShamsi(l.ProductionDate.Value) : "",

                    ItemDetailId = l.ItemDetailId,
                    ItemId = l.ItemId,
                    ManufacturerId = l.ManufacturerId,

                    Note = l.Note,
                    NumVials = l.NumVials,
                    PackageSize = l.PackageSize,
                    PackingTypeId = l.PackingTypeId,

                    Quantity = l.Quantity,
                    StoreId = l.StoreId,

                    TotalVolume = l.TotalVolume,
                    UnitVolume = l.UnitVolume,

                    UseId = l.UseId,
                    VvmStageId = l.VvmStageId,
                    VvmTypeId = l.VvmTypeId
                }).ToList(),

                NewLine = new ArrivalLineViewModel()
            };

            // Option B: if confirmed => open read-only (still show same form but disabled in view)
            ViewBag.IsReadOnly = model.IsConfirmed;

            return View(model);
        }

        public async Task<IActionResult> Edit(int id, CancellationToken token)
        {
            await LoadDropDowns(token);
            var dto = await _arrivalService.GetById(id, token);
            if (dto == null) return NotFound();

            var model = new ArrivalViewModel
            {
                ArrivalId = dto.ArrivalId,
                Date = dto.Date.HasValue ? DateTimeHelper.ToShamsi(dto.Date.Value) : "",
                ReportNumber = dto.ReportNumber,
                SourceId = dto.SourceId,
                FiscalPeriodId = dto.FiscalPeriodId,
                EntryDate = dto.EntryDate,
                IsConfirmed = dto.IsConfirmed ?? false,
                IsDrafted = dto.IsDrafted ?? false,
                IsCancelled = dto.IsCancelled ?? false,
                PackedVolume = dto.PackedVolume,
                TotalWeight = dto.TotalWeight,
                TotalVolume = dto.TotalVolume,
                Note = dto.Note,
                Frzindc = dto.Frzindc,
                FrzindcNok = dto.FrzindcNok,
                Lines = dto.Lines.Select(l => new ArrivalLineViewModel
                {
                    ArrivalLineId = l.ArrivalLineId,
                    ArrivalId = l.ArrivalId,

                    BatchNum = l.BatchNum,
                    CommertialNameId = l.CommertialNameId,
                    DosePerVialId = l.DosePerVialId,

                    ExpiryDate = l.ExpiryDate.HasValue ? DateTimeHelper.ToShamsi(l.ExpiryDate.Value) : "",
                    ProductionDate = l.ProductionDate.HasValue ? DateTimeHelper.ToShamsi(l.ProductionDate.Value) : "",

                    ItemDetailId = l.ItemDetailId,
                    ItemId = l.ItemId,
                    ManufacturerId = l.ManufacturerId,

                    Note = l.Note,
                    NumVials = l.NumVials,
                    PackageSize = l.PackageSize,
                    PackingTypeId = l.PackingTypeId,

                    Quantity = l.Quantity,
                    StoreId = l.StoreId,

                    TotalVolume = l.TotalVolume,
                    UnitVolume = l.UnitVolume,

                    UseId = l.UseId,
                    VvmStageId = l.VvmStageId,
                    VvmTypeId = l.VvmTypeId
                }).ToList(),

                NewLine = new ArrivalLineViewModel()
            };
            ViewBag.IsReadOnly = model.IsConfirmed;
            return View(model);
        }

        // MAIN POST: all buttons submit here
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ArrivalViewModel model, string command, CancellationToken token)
        {
            await LoadDropDowns(token);

            // If confirmed - block any change
            if (model.ArrivalId.HasValue)
            {
                var existing = await _arrivalService.GetById(model.ArrivalId.Value, token);
                if (existing?.IsConfirmed == true)
                {
                    TempData["ToastMessage"] = "این ورود تایید شده است و امکان ویرایش ندارد";
                    TempData["ToastType"] = "error";
                    return RedirectToAction("Edit", new { id = model.ArrivalId.Value });
                }
            }

            switch (command)
            {
                case "addLine":
                    return await AddLine(model, token);

                case "draft":
                    return await SaveDraft(model, token);

                case "confirm":
                    return await Confirm(model, token);

                case "deleteAll":
                    return await DeleteAll(model, token);

                default:
                    // fallback: treat as draft
                    return await SaveDraft(model, token);
            }
        }

        private async Task<IActionResult> AddLine(ArrivalViewModel model, CancellationToken token)
        {
            // Validate header minimum required fields (adjust to your required list)
            if (string.IsNullOrWhiteSpace(model.Date) ||
                string.IsNullOrWhiteSpace(model.ReportNumber) ||
                !model.SourceId.HasValue ||
                !model.FiscalPeriodId.HasValue)
            {
                TempData["ToastMessage"] = "ابتدا اطلاعات اصلی ورود را کامل کنید";
                TempData["ToastType"] = "error";
                return View("New", model);
            }

            // Validate NewLine minimum required (yellow fields)
            if (model.NewLine == null ||
                !model.NewLine.ItemId.HasValue ||
                !model.NewLine.StoreId.HasValue ||
                !model.NewLine.Quantity.HasValue)
            {
                TempData["ToastMessage"] = "اطلاعات کالا کامل نیست";
                TempData["ToastType"] = "error";
                return View("New", model);
            }

            // Ensure Arrival exists (create draft header if first time)
            if (!model.ArrivalId.HasValue || model.ArrivalId == 0)
            {
                var createDto = new ArrivalCreateDto
                {
                    Date = DateTimeHelper.ToMiladi(model.Date) ?? DateTime.Now,
                    ReportNumber = model.ReportNumber,
                    SourceId = model.SourceId.Value,
                    FiscalPeriodId = model.FiscalPeriodId.Value,

                    IsConfirmed = false,
                    IsDrafted = true,
                    IsCancelled = false,

                    PackedVolume = model.PackedVolume,
                    TotalWeight = model.TotalWeight,
                    Note = model.Note,
                    Frzindc = model.Frzindc,
                    FrzindcNok = model.FrzindcNok,

                    Lines = new List<ArrivalLineDto>()
                };

                var newId = await _arrivalService.Create(createDto, token);
                model.ArrivalId = newId;
            }

            // Map NewLine -> ArrivalLineDto
            var expiry = string.IsNullOrWhiteSpace(model.NewLine.ExpiryDate)
                ? (DateTime?)null
                : DateTimeHelper.ToMiladi(model.NewLine.ExpiryDate);

            var prod = string.IsNullOrWhiteSpace(model.NewLine.ProductionDate)
                ? (DateTime?)null
                : DateTimeHelper.ToMiladi(model.NewLine.ProductionDate);

            var lineDto = new ArrivalLineDto
            {
                ArrivalId = model.ArrivalId.Value,
                BatchNum = model.NewLine.BatchNum,
                CommertialNameId = model.NewLine.CommertialNameId,
                DosePerVialId = model.NewLine.DosePerVialId,

                ExpiryDate = expiry,
                ProductionDate = prod,

                ItemDetailId = model.NewLine.ItemDetailId,
                ItemId = model.NewLine.ItemId,
                ManufacturerId = model.NewLine.ManufacturerId,

                Note = model.NewLine.Note,
                NumVials = model.NewLine.NumVials,

                PackageSize = model.NewLine.PackageSize,
                PackingTypeId = model.NewLine.PackingTypeId,

                Quantity = model.NewLine.Quantity,
                StoreId = model.NewLine.StoreId,

                UnitVolume = model.NewLine.UnitVolume,
                TotalVolume = model.NewLine.TotalVolume,

                UseId = model.NewLine.UseId,
                VvmStageId = model.NewLine.VvmStageId,
                VvmTypeId = model.NewLine.VvmTypeId
            };

            await _arrivalService.AddLine(model.ArrivalId.Value, lineDto, token);

            TempData["ToastMessage"] = "کالا اضافه شد";
            TempData["ToastType"] = "success";

            // Redirect to edit mode (and NewLine will be empty again)
            return RedirectToAction("New", new { id = model.ArrivalId.Value });
        }

        private async Task<IActionResult> SaveDraft(ArrivalViewModel model, CancellationToken token)
        {
            if (!model.ArrivalId.HasValue || model.ArrivalId == 0)
            {
                TempData["ToastMessage"] = "ابتدا یک کالا اضافه کنید تا ورود ساخته شود";
                TempData["ToastType"] = "error";
                return View("New", model);
            }

            await _arrivalService.SetDraft(model.ArrivalId.Value, token);

            TempData["ToastMessage"] = "به عنوان پیش‌نویس ذخیره شد";
            TempData["ToastType"] = "info";

            return RedirectToAction("Index");
        }

        private async Task<IActionResult> Confirm(ArrivalViewModel model, CancellationToken token)
        {
            if (!model.ArrivalId.HasValue || model.ArrivalId == 0)
            {
                TempData["ToastMessage"] = "ابتدا یک کالا اضافه کنید";
                TempData["ToastType"] = "error";
                return View("New", model);
            }

            if (!model.ArrivalId.HasValue)
            {
                return await AddLine(model, token);
            }
            else
            {
                await _arrivalService.Confirm(model.ArrivalId.Value, token);
            }


            TempData["ToastMessage"] = "ورود تایید شد";
            TempData["ToastType"] = "success";

            return RedirectToAction("Index");
        }

        private async Task<IActionResult> DeleteAll(ArrivalViewModel model, CancellationToken token)
        {
            if (!model.ArrivalId.HasValue || model.ArrivalId == 0)
                return RedirectToAction("Index");

            await _arrivalService.Cancel(model.ArrivalId.Value, token);

            TempData["ToastMessage"] = "کل ورود حذف شد";
            TempData["ToastType"] = "success";

            return RedirectToAction("Index");
        }

        // optional: delete one line from grid
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLine(int id, int arrivalId, CancellationToken token)
        {
            await _arrivalService.CancelLine(id, token);

            TempData["ToastMessage"] = "سطر حذف شد";
            TempData["ToastType"] = "success";

            return RedirectToAction("Edit", new { id = arrivalId });
        }

        private async Task LoadDropDowns(CancellationToken token)
        {
            ViewBag.SourceList = (await _sourceService.GetAll(token))
                .Select(x => new SelectListItem { Value = x.SourceId.ToString(), Text = x.FullName })
                .ToList();

            ViewBag.FiscalPeriodList = (await _fiscalService.GetActives(token))
                .Select(x => new SelectListItem { Value = x.FiscalPeriodId.ToString(), Text = x.FullName })
                .ToList();

            ViewBag.ItemList = (await _itemService.GetActives(token))
                .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.FullName })
                .ToList();

            ViewBag.StoreList = (await _storeService.GetAll(token))
                .Select(x => new SelectListItem { Value = x.StoreId.ToString(), Text = x.FullName })
                .ToList();

            ViewBag.ManufacturerList = (await _manufacturerService.GetActives(token))
                .Select(x => new SelectListItem { Value = x.ManufacturerId.ToString(), Text = x.FullName })
                .ToList();

            ViewBag.DosePerVialList = (await _dosePerVialService.GetActives(token))
                .Select(x => new SelectListItem { Value = x.DosePerVialId.ToString(), Text = x.NumDosePerVial.ToString() })
                .ToList();

            ViewBag.VvmtypeList = (await _vvmtypeService.GetActives(token))
                .Select(x => new SelectListItem { Value = x.VvmtypeId.ToString(), Text = x.FullName })
                .ToList();

            ViewBag.VvmstageList = (await _vvmstageService.GetActives(token))
                .Select(x => new SelectListItem { Value = x.VvmstageId.ToString(), Text = x.FullName })
                .ToList();

            ViewBag.CommercialNameList = (await _commercialNameService.GetActives(token))
                .Select(x => new SelectListItem { Value = x.CommercialNameId.ToString(), Text = x.FullName })
                .ToList();

            ViewBag.ItemDetailList = (await _itemDetailService.GetActives(token))
                .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name.ToString() })
                .ToList();

            ViewBag.UseList = (await _useService.GetActives(token))
                .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.FullName })
                .ToList();

            ViewBag.PackingTypeList = (await _packingTypeService.GetActives(token))
                .Select(x => new SelectListItem { Value = x.PackingTypeId.ToString(), Text = x.FullName })
                .ToList();
        }
        public async Task<IActionResult> Copy(string? reportNumber, CancellationToken token)
        {
            await LoadDropDowns(token);
            ViewBag.Arrivals = (await _arrivalService.GetForCopy(reportNumber, token))
                .Select(x => new SelectListItem
                {
                    Value = x.ArrivalId.ToString(),
                    Text = x.DisplayText
                })
                .ToList();

            ViewBag.Search = reportNumber;
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> CopyConfirm(int arrivalId, CancellationToken token)
        {
            await LoadDropDowns(token);
            var dto = await _arrivalService.GetById(arrivalId, token);
            if (dto == null) return NotFound();

            var vm = new ArrivalViewModel
            {
                ArrivalId = null,                // ❌ important
                Date = dto.Date.Value.ToShamsi(),
                ReportNumber = dto.ReportNumber,
                SourceId = dto.SourceId,
                FiscalPeriodId = dto.FiscalPeriodId,
                Note = $"{dto.ReportNumber} کپی ورودی شماره ",
                PackedVolume = dto.PackedVolume,
                TotalWeight = dto.TotalWeight,
                TotalVolume = dto.TotalVolume,
                IsDrafted = true,               // ⬅ always draft after copy
                IsConfirmed = false,
                IsCancelled = false,

                Lines = dto.Lines.Select(l => new ArrivalLineViewModel
                {
                    ArrivalLineId = null,
                    ArrivalId = null,
                    BatchNum = l.BatchNum,
                    CommertialNameId = l.CommertialNameId,
                    DosePerVialId = l.DosePerVialId,
                    ExpiryDate = l.ExpiryDate.HasValue ? DateTimeHelper.ToShamsi(l.ExpiryDate.Value) : "",
                    ProductionDate = l.ProductionDate.HasValue ? DateTimeHelper.ToShamsi(l.ProductionDate.Value) : "",
                    ItemDetailId = l.ItemDetailId,
                    ItemId = l.ItemId,
                    ManufacturerId = l.ManufacturerId,
                    Note = l.Note,
                    NumVials = l.NumVials,
                    PackageSize = l.PackageSize,
                    PackingTypeId = l.PackingTypeId,
                    Quantity = l.Quantity,
                    StoreId = l.StoreId,
                    TotalVolume = l.TotalVolume,
                    UnitVolume = l.UnitVolume,
                    UseId = l.UseId,
                    VvmStageId = l.VvmStageId,
                    VvmTypeId = l.VvmTypeId
                }).ToList()
            };

            ModelState.Remove(nameof(ArrivalViewModel.ArrivalId));
            await AddLineForCopy(vm, token);
            ViewBag.Status = 1;
            return await Index(null);

        }

        private async Task<IActionResult> AddLineForCopy(ArrivalViewModel model, CancellationToken token)
        {




            // Ensure Arrival exists (create draft header if first time)
            if (!model.ArrivalId.HasValue || model.ArrivalId == 0)
            {
                var createDto = new ArrivalCreateDto
                {
                    Date = DateTimeHelper.ToMiladi(model.Date) ?? DateTime.Now,
                    ReportNumber = model.ReportNumber,
                    SourceId = model.SourceId.Value,
                    FiscalPeriodId = model.FiscalPeriodId.Value,

                    IsConfirmed = false,
                    IsDrafted = true,
                    IsCancelled = false,

                    PackedVolume = model.PackedVolume,
                    TotalWeight = model.TotalWeight,
                    Note = model.Note,
                    Frzindc = model.Frzindc,
                    FrzindcNok = model.FrzindcNok,

                    Lines = new List<ArrivalLineDto>()
                };

                var newId = await _arrivalService.Create(createDto, token);
                model.ArrivalId = newId;
            }

            foreach (var line in model.Lines)
            {

                // Map NewLine -> ArrivalLineDto
                var expiry = string.IsNullOrWhiteSpace(model.NewLine.ExpiryDate)
                    ? (DateTime?)null
                    : DateTimeHelper.ToMiladi(model.NewLine.ExpiryDate);

                var prod = string.IsNullOrWhiteSpace(model.NewLine.ProductionDate)
                    ? (DateTime?)null
                    : DateTimeHelper.ToMiladi(model.NewLine.ProductionDate);

                var lineDto = new ArrivalLineDto
                {
                    ArrivalId = model.ArrivalId.Value,
                    BatchNum = line.BatchNum,
                    CommertialNameId = line.CommertialNameId,
                    DosePerVialId = line.DosePerVialId,

                    ExpiryDate = expiry,
                    ProductionDate = prod,

                    ItemDetailId = line.ItemDetailId,
                    ItemId = line.ItemId,
                    ManufacturerId = line.ManufacturerId,

                    Note = line.Note,
                    NumVials = line.NumVials,

                    PackageSize = line.PackageSize,
                    PackingTypeId = line.PackingTypeId,

                    Quantity = line.Quantity,
                    StoreId = line.StoreId,

                    UnitVolume = line.UnitVolume,
                    TotalVolume = line.TotalVolume,

                    UseId = line.UseId,
                    VvmStageId = line.VvmStageId,
                    VvmTypeId = line.VvmTypeId
                };
                await _arrivalService.AddLine(model.ArrivalId.Value, lineDto, token);
            }


            TempData["ToastMessage"] = "کالا اضافه شد";
            TempData["ToastType"] = "success";

            // Redirect to edit mode (and NewLine will be empty again)
            return RedirectToAction("New", new { id = model.ArrivalId.Value });
        }

    }
}
