using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using VSSM.Application.DTOs.BaseEntities.Arrival;
using VSSM.Application.Helpers;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.Arrival;
using VSSM.Presentation.Web.Models.Base;

namespace VSSM.Presentation.Web.Controllers;

public class ArrivalController(
    IArrivalService arrivalService,
    IFiscalPeriodService fiscalService,
    IItemService itemService,
    IStoreService storeService,
    IDosePerVialService dosePerVialService,
    ICommercialNameService commercialNameService,
    IBaseInfoService baseInfoService,
    IItemClassService itemClassService,
    IOrderInfoService orderInfoService,
    IUidInfoService uidInfoService) : BaseController
{
    public async Task<IActionResult> Index(
        string? search,
        int status = 3,
        int page = 1,
        int pageSize = 10,
        string sort = "ArrivalId",
        string direction = "desc",
        CancellationToken token = default)
    {
        var list = await arrivalService.GetPagedList(search, status, page, pageSize, token);

        var model = new GridViewModel
        {
            SearchTerm = search,
            CurrentSort = sort,
            CurrentDirection = direction,
            Data = new PagedResult<object>
            {
                Items = list.Items.Cast<object>().ToList(),
                PageNumber = list.Paging.Page,
                PageSize = pageSize,
                TotalCount = list.Paging.TotalPages * pageSize
            },
            Columns = new List<GridColumn>
            {
                new() { Header = "دوره فعالیت", PropertyName = nameof(ArrivalListDto.FiscalPeriodName), Sortable = false },
                new() { Header = "تاریخ ورود",  PropertyName = nameof(ArrivalListDto.DateText),         Sortable = false },
                new() { Header = "شماره گزارش", PropertyName = nameof(ArrivalListDto.ReportNumber),     Sortable = false },
                new() { Header = "وضعیت",       PropertyName = nameof(ArrivalListDto.StatusText),       Sortable = false },
                new() { Header = "عملیات",      IsActionColumn = true }
            },
            Actions = new List<GridAction>
            {
                new()
                {
                    Title           = "ویرایش",
                    Controller      = "Arrival",
                    Action          = "New",
                    RouteIdProperty = nameof(ArrivalListDto.ArrivalId),
                    IconClass       = "fa fa-edit",
                    LinkClass       = "",
                    DynamicTitle    = item =>
                    {
                        var dto = item as ArrivalListDto;
                        return dto?.IsDrafted == true ? "ویرایش" : "";
                    }
                },
                new()
                {
                    Title           = "چاپ",
                    Controller      = "Arrival",
                    Action          = "Print",
                    RouteIdProperty = nameof(ArrivalListDto.ArrivalId),
                    IconClass       = "fa fa-print",
                    LinkClass       = "",
                    DynamicTitle    = item =>
                    {
                        var dto = item as ArrivalListDto;
                        return dto?.IsConfirmed == true ? "چاپ" : "";
                    }
                },
                new()
                {
                    Title           = "حذف",
                    Controller      = "Arrival",
                    Action          = "Delete",
                    RouteIdProperty = nameof(ArrivalListDto.ArrivalId),
                    IconClass       = "fa fa-trash",
                    LinkClass       = "",
                    DynamicTitle    = item =>
                    {
                        var dto = item as ArrivalListDto;
                        return dto?.IsDrafted == true ? "حذف" : "";
                    }
                }
            }
        };

        return View(model);
    }


    public IActionResult Print(int id)
    {
        return View("~/Views/Shared/UnderConstruction.cshtml");
    }
    public async Task<IActionResult> New(int? id, CancellationToken token)
    {
        await LoadDropDowns(token);

        if (id == null || id == 0)
        {
            var vm = new ArrivalViewModel
            {
                Date = DateTime.Now.ToShamsi(),
                IsDrafted = true,
                Lines = new List<ArrivalLineViewModel>(),
                NewLine = new ArrivalLineViewModel()
            };
            return View(vm);
        }

        var dto = await arrivalService.GetById(id.Value, token);
        if (dto == null) return NotFound();

        var model = new ArrivalViewModel
        {
            ArrivalId = dto.ArrivalId,
            Date = dto.Date.HasValue ? DateTimeHelper.ToShamsi(dto.Date.Value) : "",
            ReportNumber = dto.ReportNumber,
            FiscalPeriodName = fiscalService.GetActives(token).Result.FirstOrDefault()?.FullName,
            EntryDate = dto.EntryDate,
            IsConfirmed = dto.IsConfirmed ?? false,
            IsDrafted = dto.IsDrafted ?? false,
            IsCancelled = dto.IsCancelled ?? false,
            Note = dto.Note,
            Lines = dto.Lines.Select(l => MapLineToViewModel(l, token)).ToList(),
            NewLine = new ArrivalLineViewModel()
        };

        ViewBag.IsReadOnly = model.IsConfirmed;
        return View(model);
    }

    public async Task<IActionResult> Delete(int id, CancellationToken token)
    {
        // Step 6: قبل از حذف، uid هایی که arrival_fk_id == id دارند را null کن
        await uidInfoService.ClearArrivalFkByArrivalId(id, token);

        await arrivalService.Delete(id);

        TempData["ToastMessage"] = "ورود حذف شد";
        TempData["ToastType"] = "success";
        return RedirectToAction(nameof(Index));
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ArrivalViewModel model, string command, CancellationToken token)
    {
        try
        {
            await LoadDropDowns(token);

            if (command != null && (
                command.StartsWith("deleteLine_", StringComparison.OrdinalIgnoreCase) ||
                command.StartsWith("editLine_", StringComparison.OrdinalIgnoreCase) ||
                command == "draft" ||
                command == "confirm"))
            {
                ModelState.Clear();
            }

            if (model.ArrivalId.HasValue)
            {
                var existing = await arrivalService.GetById(model.ArrivalId.Value, token);
                if (existing?.IsConfirmed == true)
                {
                    TempData["ToastMessage"] = "این ورود تایید شده است و امکان ویرایش ندارد";
                    TempData["ToastType"] = "error";
                    return RedirectToAction("New", new { id = model.ArrivalId.Value });
                }
            }

            return command switch
            {
                "addLine" => await AddLine(model, token),
                "draft" => await SaveDraft(model, token),
                "confirm" => await Confirm(model, token),
                "deleteAll" => await DeleteAll(model, token),
                string s when s.StartsWith("deleteLine_", StringComparison.OrdinalIgnoreCase)
                => await HandleDeleteLine(s, model, token),
                string s when s.StartsWith("editLine_", StringComparison.OrdinalIgnoreCase)
                => await HandleEditLine(s, model, token),

                _ => await SaveDraft(model, token)
            };
        }
        catch (Exception ex)
        {
            TempData["ToastMessage"] = ex.Message;
            TempData["ToastType"] = "error";
            return View("New", model);
        }
    }

    private async Task<IActionResult> HandleEditLine(string command, ArrivalViewModel model, CancellationToken token)
    {
        var idPart = command.Split('_').Last();
        if (!int.TryParse(idPart, out var lineId))
        {
            TempData["ToastMessage"] = "شناسه سطر معتبر نیست";
            TempData["ToastType"] = "error";
            return View("New", model);
        }

        if (!model.ArrivalId.HasValue || model.ArrivalId.Value == 0)
        {
            TempData["ToastMessage"] = "شناسه ورود معتبر نیست";
            TempData["ToastType"] = "error";
            return View("New", model);
        }

        var dto = await arrivalService.GetById(model.ArrivalId.Value, token);
        var line = dto?.Lines.FirstOrDefault(l => l.ArrivalLineId == lineId);
        if (line == null)
        {
            TempData["ToastMessage"] = "سطر مورد نظر یافت نشد";
            TempData["ToastType"] = "error";
            return View("New", model);
        }

        await uidInfoService.ClearArrivalFkByLineId(lineId, token);
        await arrivalService.CancelLine(lineId, token);

        await LoadDropDowns(token);

        var updated = await arrivalService.GetById(model.ArrivalId.Value, token);
        model.Lines = updated?.Lines.Select(l => MapLineToViewModel(l, token)).ToList()
                      ?? new List<ArrivalLineViewModel>();

        model.NewLine = MapLineToViewModel(line, token);
        model.EditingLineId = lineId;

        return View("New", model);
    }


    private async Task<IActionResult> AddLine(ArrivalViewModel model, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(model.Date))
        {
            TempData["ToastMessage"] = "فیلد تاریخ ورود اجباری است";
            TempData["ToastType"] = "error";
            return View("New", model);
        }

        if (model.NewLine == null || string.IsNullOrEmpty(model.NewLine.BatchNum))
        {
            TempData["ToastMessage"] = "فیلد سری ساخت اجباری است";
            TempData["ToastType"] = "error";
            return View("New", model);
        }

        if (!model.NewLine.StoreId.HasValue)
        {
            TempData["ToastMessage"] = "فیلد انبار اجباری است";
            TempData["ToastType"] = "error";
            return View("New", model);
        }

        if (!model.NewLine.ItemId.HasValue)
        {
            TempData["ToastMessage"] = "فیلد کالا اجباری است";
            TempData["ToastType"] = "error";
            return View("New", model);
        }

        if (!model.NewLine.DosePerVialId.HasValue)
        {
            TempData["ToastMessage"] = "فیلد دوز بر ویال اجباری است";
            TempData["ToastType"] = "error";
            return View("New", model);
        }

        var batchNum = model.NewLine.BatchNum.Trim();

        if (model.Lines != null && model.Lines.Any(l =>
            string.Equals(l.BatchNum, batchNum, StringComparison.OrdinalIgnoreCase)))
        {
            TempData["ToastMessage"] = "سری ساخت تکراری است";
            TempData["ToastType"] = "error";
            return View("New", model);
        }

        var orderInfo = await orderInfoService.GetConfirmedByBatchNum(batchNum, token);
        if (orderInfo == null)
        {
            TempData["ToastMessage"] = "فایل مربوط به سری ساخت مورد نظر آپلود نشده است";
            TempData["ToastType"] = "error";
            return View("New", model);
        }

        // Step 3: بررسی وجود uid با arrival_fk_id خالی
        var availableUids = await uidInfoService.GetAvailableByBatchNum(
            batchNum,
            CurrentUserRecipientId ?? 0,
            token);

        if (!availableUids.Any())
        {
            TempData["ToastMessage"] = "کلیه ویال‌های مربوط به سری ساخت وارد شده ورود خورده‌اند";
            TempData["ToastType"] = "error";
            return View("New", model);
        }

        model.NewLine.ExpiryDate = orderInfo.ExpirationDate;
        model.NewLine.ProductionDate = orderInfo.ManufactureDate;
        model.NewLine.Quantity = availableUids.Count;

        if (!model.ArrivalId.HasValue || model.ArrivalId == 0)
        {
            var createDto = BuildCreateDto(model);
            var newId = await arrivalService.Create(createDto, token);
            model.ArrivalId = newId;
        }

        var lineDto = MapNewLineToDto(model);
        await arrivalService.AddLine(model.ArrivalId.Value, lineDto, token);

        var itemInfo = await itemService.GetById(model.NewLine.ItemId.Value, token);
        if (itemInfo?.RequiresDiluent == true && itemInfo.DiluentId.HasValue)
        {
            var diluentLineDto = new ArrivalLineDto
            {
                ArrivalId = model.ArrivalId.Value,
                BatchNum = model.NewLine.BatchNum,
                StoreId = model.NewLine.StoreId,
                ItemId = itemInfo.DiluentId.Value,
                DosePerVialId = model.NewLine.DosePerVialId,
                Quantity = model.NewLine.Quantity,
                ExpiryDate = model.NewLine.ExpiryDate,
                ProductionDate = model.NewLine.ProductionDate,
                Note = "حلال خودکار",
            };
            await arrivalService.AddLine(model.ArrivalId.Value, diluentLineDto, token);
        }

        return RedirectToAction("New", new { id = model.ArrivalId.Value });
    }

    [HttpGet]
    public async Task<IActionResult> GetBatchInfo(string batchNum, int? arrivalId, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(batchNum))
            return Json(new { success = false, message = "سری ساخت خالی است" });

        batchNum = batchNum.Trim();

        var orderInfo = await orderInfoService.GetConfirmedByBatchNum(batchNum, token);
        if (orderInfo == null)
            return Json(new { success = false, message = "فایل مربوط به سری ساخت مورد نظر آپلود نشده است" });

        var availableUids = await uidInfoService.GetAvailableByBatchNum(
            batchNum,
            CurrentUserRecipientId ?? 0,
            token);

        if (!availableUids.Any())
            return Json(new { success = false, message = "کلیه ویال‌های مربوط به سری ساخت وارد شده ورود خورده‌اند" });

        var productionDateShamsi = orderInfo.ManufactureDate?.ToShamsi();
        var productionDateMiladi = orderInfo.ManufactureDate.HasValue
            ? orderInfo.ManufactureDate.Value.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture)
            : null;

        var expiryDateShamsi = orderInfo.ExpirationDate?.ToShamsi();
        var expiryDateMiladi = orderInfo.ExpirationDate.HasValue
            ? orderInfo.ExpirationDate.Value.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture)
            : null;

        return Json(new
        {
            success = true,
            quantity = availableUids.Count,

            productionDateShamsi,
            productionDateMiladi,
            hasProdShamsi = !string.IsNullOrEmpty(productionDateShamsi),
            hasProdMiladi = !string.IsNullOrEmpty(productionDateMiladi),

            expiryDateShamsi,
            expiryDateMiladi,
            hasExpShamsi = !string.IsNullOrEmpty(expiryDateShamsi),
            hasExpMiladi = !string.IsNullOrEmpty(expiryDateMiladi),
            hasExpiry = orderInfo.ExpirationDate.HasValue,

            itemId = orderInfo.FkItemId,
            itemName = orderInfo.ItemName,
            dosePerVialId = orderInfo.FkDosePerVialId,
            doseName = orderInfo.DoseName,
            itemClassName = "واکسن"
        });
    }


    private async Task<IActionResult> SaveDraft(ArrivalViewModel model, CancellationToken token)
    {
        if (!model.ArrivalId.HasValue || model.ArrivalId == 0)
        {
            TempData["ToastMessage"] = "ابتدا یک کالا اضافه کنید تا ورود ساخته شود";
            TempData["ToastType"] = "error";
            return View("New", model);
        }

        if (!await arrivalService.DoesHaveArrivalLineByArrivalId(model.ArrivalId.Value))
        {
            TempData["ToastMessage"] = "ابتدا یک کالا اضافه کنید تا ورود ساخته شود";
            TempData["ToastType"] = "error";
            return View("New", model);
        }

        await arrivalService.SetDraft(model.ArrivalId.Value, BuildCreateDto(model), token);

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

        if (!await arrivalService.DoesHaveArrivalLineByArrivalId(model.ArrivalId.Value))
        {
            TempData["ToastMessage"] = "ابتدا یک کالا اضافه کنید";
            TempData["ToastType"] = "error";
            return View("New", model);
        }

        await arrivalService.Confirm(model.ArrivalId.Value, BuildCreateDto(model), token);

        var dto = await arrivalService.GetById(model.ArrivalId.Value, token);

        // برای هر سطر جداگانه، با StoreId همان سطر
        foreach (var line in dto?.Lines ?? new List<ArrivalLineDto>())
        {
            if (string.IsNullOrWhiteSpace(line.BatchNum)) continue;

            await uidInfoService.SetArrivalFkForBatchNums(
                new List<string> { line.BatchNum },
                model.ArrivalId.Value,
                line.DosePerVialId,
                line.StoreId,   // StoreId همین سطر، نه firstLine
                line.ItemId,
                token);
        }

        TempData["ToastMessage"] = "ورود تایید شد";
        TempData["ToastType"] = "success";
        return RedirectToAction("Index");
    }
    private async Task<IActionResult> DeleteAll(ArrivalViewModel model, CancellationToken token)
    {
        if (!model.ArrivalId.HasValue || model.ArrivalId == 0)
            return RedirectToAction("Index");

        await uidInfoService.ClearArrivalFkByArrivalId(model.ArrivalId.Value, token);
        await arrivalService.Cancel(model.ArrivalId.Value, token);

        TempData["ToastMessage"] = "کل ورود حذف شد";
        TempData["ToastType"] = "success";
        return RedirectToAction("Index");
    }

    private async Task<IActionResult> HandleDeleteLine(string command, ArrivalViewModel model, CancellationToken token)
    {
        var idPart = command.Split('_').Last();
        if (!int.TryParse(idPart, out var lineId))
        {
            TempData["ToastMessage"] = "شناسه سطر معتبر نیست";
            TempData["ToastType"] = "error";
            return View("New", model);
        }

        if (!model.ArrivalId.HasValue || model.ArrivalId.Value == 0)
        {
            TempData["ToastMessage"] = "شناسه ورود معتبر نیست";
            TempData["ToastType"] = "error";
            return View("New", model);
        }

        return await DeleteLine(lineId, model.ArrivalId.Value, token);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLine(int id, int arrivalId, CancellationToken token)
    {
        // Step 6: null کردن arrival_fk_id در uid های مربوط به این line
        await uidInfoService.ClearArrivalFkByLineId(id, token);

        await arrivalService.CancelLine(id, token);

        TempData["ToastMessage"] = "سطر حذف شد";
        TempData["ToastType"] = "success";
        return RedirectToAction("New", new { id = arrivalId });
    }

    [HttpGet]
    public async Task<IActionResult> GetItemsByClass(int classId, CancellationToken token)
    {
        var list = (await itemService.GetActives(token)).Where(z => z.FkClassId == classId);
        return Json(list.Select(x => new { id = x.ItemId, text = x.FullName }));
    }

    [HttpGet]
    public async Task<IActionResult> GetDosePerVialsByItem(int itemId, CancellationToken token)
    {
        var list = (await dosePerVialService.GetActivesByItemId(token, itemId))
            .OrderBy(z => z.NumDosePerVial)
            .Select(x => new { value = x.DosePerVialId, text = x.NumDosePerVial.ToString() });
        return Json(list);
    }

    [HttpGet]
    public async Task<IActionResult> GetVvmStages(int vvmTypeId, CancellationToken token)
    {
        var stages = await baseInfoService.GetByParentIdAsync(30, token);
        return Json(stages.Select(x => new { id = x.Id, text = x.Title }));
    }

    private ArrivalCreateDto BuildCreateDto(ArrivalViewModel model) => new()
    {
        Date = DateTimeHelper.ToMiladi(model.Date) ?? DateTime.Now,
        ReportNumber = model.ReportNumber,
        FiscalPeriodId = fiscalService.GetActives(default).Result.First().FiscalPeriodId,
        IsConfirmed = false,
        IsDrafted = true,
        IsCancelled = false,
        EnterById = CurrentUserId.ToString(),
        Note = model.Note,
        UserRecipientId = CurrentUserRecipientId ?? 0,
    };

    private ArrivalLineDto MapNewLineToDto(ArrivalViewModel model) => new()
    {
        ArrivalId = model.ArrivalId!.Value,
        BatchNum = model.NewLine.BatchNum,
        CommertialNameId = model.NewLine.CommertialNameId,
        DosePerVialId = model.NewLine.DosePerVialId,
        ExpiryDate = model.NewLine.ExpiryDate,
        ProductionDate = model.NewLine.ProductionDate,
        ItemDetailId = model.NewLine.ItemDetailId,
        ItemId = model.NewLine.ItemId,
        ManufacturerId = model.NewLine.ManufacturerId,
        Note = model.NewLine.Note,
        NumVials = model.NewLine.NumVials,
        Quantity = model.NewLine.Quantity,
        StoreId = model.NewLine.StoreId,
        UseId = model.NewLine.UseId,
        VvmStageId = model.NewLine.VvmStageId,
        VvmTypeId = model.NewLine.VvmTypeId,
    };

    private ArrivalLineViewModel MapLineToViewModel(ArrivalLineDto l, CancellationToken token) => new()
    {
        ArrivalLineId = l.ArrivalLineId,
        ArrivalId = l.ArrivalId,
        BatchNum = l.BatchNum,
        CommertialNameId = l.CommertialNameId,
        CommertialName = commercialNameService.GetActives(token).Result
                               .FirstOrDefault(z => z.CommercialNameId == l.CommertialNameId)?.FullName,
        ItemName = itemService.GetActives(token).Result
                               .FirstOrDefault(z => z.ItemId == l.ItemId)?.FullName,
        DoseName = dosePerVialService.GetActivesByItemId(token, l.ItemId ?? 0).Result
                   .FirstOrDefault(z => z.DosePerVialId == l.DosePerVialId)?.NumDosePerVial.ToString(),
        DosePerVialId = l.DosePerVialId,
        ExpiryDate = l.ExpiryDate,
        ProductionDate = l.ProductionDate,
        ExpiryDateShamsi = l.ExpiryDate?.ToShamsi(),
        ProductionDateShamsi = l.ProductionDate?.ToShamsi(),
        ItemDetailId = l.ItemDetailId,
        ItemId = l.ItemId,
        ManufacturerId = l.ManufacturerId,
        Note = l.Note,
        NumVials = l.NumVials,
        Quantity = l.Quantity,
        StoreId = l.StoreId,
        StoreName = storeService.GetAll(token).Result
                               .FirstOrDefault(z => z.StoreId == l.StoreId)?.FullName,
        UseId = l.UseId,
        VvmStageId = l.VvmStageId,
        VvmTypeId = l.VvmTypeId,
    };

    private async Task LoadDropDowns(CancellationToken token)
    {
        ViewBag.ItemList = (await itemService.GetActives(token))
            .Select(x => new SelectListItem { Value = x.ItemId.ToString(), Text = x.FullName })
            .ToList();

        ViewBag.StoreList = (await storeService.GetAll(token))
            .Where(z => z.RecipientId == CurrentUserRecipientId)
            .Select(x => new SelectListItem { Value = x.StoreId.ToString(), Text = x.FullName })
            .ToList();

        ViewBag.DosePerVialList = new List<SelectListItem>();

        ViewBag.VvmTypeList = (await baseInfoService.GetByParentIdAsync(22, token))
            .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Title })
            .ToList();

        ViewBag.VvmstageList = (await baseInfoService.GetByParentIdAsync(30, token))
            .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Title })
            .ToList();

        ViewBag.ManufacturerList = (await baseInfoService.GetByParentIdAsync(48, token))
            .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Title })
            .ToList();

        ViewBag.CommercialNameList = (await baseInfoService.GetByParentIdAsync(35, token))
            .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Title })
            .ToList();

        ViewBag.UseList = (await baseInfoService.GetByParentIdAsync(7, token))
            .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Title })
            .ToList();

        ViewBag.itemClassList = (await itemClassService.GetActives(token))
            .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name })
            .ToList();
    }

    //private async Task LoadDropDowns(CancellationToken token)
    //{
    //    ViewBag.ItemList = (await itemService.GetActives(token))
    //        .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.FullName })
    //        .ToList();

    //    ViewBag.StoreList = (await storeService.GetAll(token))
    //        .Where(z => z.RecipientId == CurrentUserRecipientId)
    //        .Select(x => new SelectListItem { Value = x.StoreId.ToString(), Text = x.FullName })
    //        .ToList();

    //    ViewBag.ManufacturerList = (await manufacturerService.GetActives(token))
    //        .Select(x => new SelectListItem { Value = x.ManufacturerId.ToString(), Text = x.FullName })
    //        .ToList();

    //    ViewBag.DosePerVialList = new List<SelectListItem>();

    //    ViewBag.VvmTypeList = (await vvmtypeService.GetActives(token))
    //        .Select(x => new SelectListItem { Value = x.VvmtypeId.ToString(), Text = x.FullName })
    //        .ToList();

    //    ViewBag.VvmstageList = (await vvmstageService.GetActives(token))
    //        .Select(x => new SelectListItem { Value = x.VvmstageId.ToString(), Text = x.FullName })
    //        .ToList();

    //    ViewBag.CommercialNameList = (await commercialNameService.GetActives(token))
    //        .Select(x => new SelectListItem { Value = x.CommercialNameId.ToString(), Text = x.FullName })
    //        .ToList();

    //    ViewBag.UseList = (await useService.GetActives(token))
    //        .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.FullName })
    //        .ToList();

    //    ViewBag.itemClassList = (await itemClassService.GetActives(token))
    //        .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name })
    //        .ToList();
    //}
}