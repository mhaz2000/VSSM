using DocumentFormat.OpenXml.Validation;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using VSSM.Application.DTOs.BaseEntities.Arrival;
using VSSM.Application.Helpers;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Const;
using VSSM.Presentation.Web.Extensions;
using VSSM.Presentation.Web.Models.Arrival;
using VSSM.Presentation.Web.Models.Base;

namespace VSSM.Presentation.Web.Controllers;

public class ArrivalController(
    IArrivalService arrivalService,
    IFiscalPeriodService fiscalService,
    IItemService itemService,
    IStoreService storeService,
    IBaseInfoService baseInfoService,
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
                new()
                {
                    Header         = "وضعیت",
                    PropertyName   = nameof(ArrivalListDto.StatusText),
                    Sortable       = false,
                    IsBadgeColumn  = true,
                    BadgeMap       = new Dictionary<string, (string Text, string CssClass)>
                    {
                        ["حذف شده"]  = ("حذف شده",  "badge-danger"),
                        ["تایید شده"] = ("تایید شده", "badge-success"),
                        ["پیش‌نویس"]  = ("پیش‌نویس",  "badge-warning"),
                        ["نامشخص"]    = ("نامشخص",    "badge-secondary")
                    }
                },
                new() { Header = "عملیات", IsActionColumn = true }
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
                    DynamicTitle    = item => (item as ArrivalListDto)?.IsDrafted == true ? "ویرایش" : ""
                },
                new()
                {
                    Title           = "چاپ",
                    Controller      = "Arrival",
                    Action          = "Print",
                    RouteIdProperty = nameof(ArrivalListDto.ArrivalId),
                    IconClass       = "fa fa-print",
                    DynamicTitle    = item => (item as ArrivalListDto)?.IsConfirmed == true ? "چاپ" : ""
                },
                new()
                {
                    Title           = "حذف",
                    Controller      = "Arrival",
                    Action          = "Delete",
                    RouteIdProperty = nameof(ArrivalListDto.ArrivalId),
                    IconClass       = "fa fa-trash",
                    DynamicTitle    = item => (item as ArrivalListDto)?.IsDrafted == true ? "حذف" : ""
                }
            }
        };

        return View(model);
    }

    public IActionResult Print(int id)
    {
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("~/Views/Shared/UnderConstructionModal.cshtml");
        }

        return View("~/Views/Shared/UnderConstruction.cshtml");
    }

    public async Task<IActionResult> New(int? id, string? tab, CancellationToken token)
    {
        await PopulateLookupViewBagsAsync(token);

        string activeTab = tab ?? "vaccine";

        if (id is null or 0)
        {
            var reportNumber = await arrivalService.Generate();
            return View(new ArrivalViewModel
            {
                Date = DateTime.Now.ToShamsi(),
                IsDrafted = true,
                Lines = new List<ArrivalLineViewModel>(),
                NewLine = new ArrivalLineViewModel(),
                ReportNumber = reportNumber,
                ArrivalType = activeTab
            });
        }

        var dto = await arrivalService.GetById(id.Value, token);
        if (dto == null) return NotFound();

        var lineViewModels = await MapLinesToViewModelAsync(dto.Lines, token);

         var arrivalType = !string.IsNullOrEmpty(tab) ? tab : dto.ArrivalType;

        var model = new ArrivalViewModel
        {
            ArrivalId = dto.ArrivalId,
            Date = dto.Date.HasValue ? DateTimeHelper.ToShamsi(dto.Date.Value) : "",
            ReportNumber = dto.ReportNumber,
            FiscalPeriodName = (await fiscalService.GetActives(token)).FirstOrDefault()?.FullName,
            EntryDate = dto.EntryDate,
            IsConfirmed = dto.IsConfirmed ?? false,
            IsDrafted = dto.IsDrafted ?? false,
            IsCancelled = dto.IsCancelled ?? false,
            Note = dto.Note,
            ArrivalType = arrivalType,  
            Lines = lineViewModels,
            NewLine = new ArrivalLineViewModel()
        };

        ViewBag.IsReadOnly = model.IsConfirmed;
        return View(model);
    }

    public async Task<IActionResult> Delete(int id, CancellationToken token)
    {
        await uidInfoService.ClearArrivalFkByArrivalId(id, token);
        await arrivalService.Delete(id);

        TempData["ToastMessage"] = "ورود حذف شد";
        TempData["ToastType"] = "success";
        return RedirectToAction(nameof(Index));
    }


    [HttpPost]
    [IgnoreAntiforgeryToken]  //  موقتاً
    public async Task<IActionResult> Create(ArrivalViewModel model, string command, CancellationToken token)
    {
        try
        {

            //await HttpContext.RequestServices.GetRequiredService<IAntiforgery>()
            //         .ValidateRequestAsync(HttpContext);

            await PopulateLookupViewBagsAsync(token);

            if (command is not null && (
                command.StartsWith("deleteLine_", StringComparison.OrdinalIgnoreCase) ||
                command.StartsWith("editLine_", StringComparison.OrdinalIgnoreCase) ||
                command is "draft" or "confirm"))
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


    private async Task<IActionResult> AddLine(ArrivalViewModel model, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(model.Date))
            return ToastView("فیلد تاریخ ورود اجباری است", model);

        if (!model.NewLine.StoreId.HasValue)
            return ToastView("فیلد انبار اجباری است", model);

        var isHalal = string.Equals(model.ArrivalType, "halal", StringComparison.OrdinalIgnoreCase);

        return isHalal
            ? await AddHalalLine(model, token)
            : await AddVaccineLine(model, token);
    }

    private async Task<IActionResult> AddVaccineLine(ArrivalViewModel model, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(model.NewLine?.BatchNum))
            return ToastView("فیلد سری ساخت اجباری است", model);


        if (!model.NewLine.ItemId.HasValue && model.NewLine.ItemId.HasValue)
            model.NewLine.ItemId = model.NewLine.ItemId;

        if (!model.NewLine.ItemId.HasValue)
            return ToastView("فیلد کالا اجباری است", model);

        if (!model.NewLine.DosePerVialId.HasValue)
            return ToastView("فیلد دوز بر ویال اجباری است", model);

        var batchNum = model.NewLine.BatchNum.Trim();

        if (model.Lines?.Any(l => string.Equals(l.BatchNum, batchNum, StringComparison.OrdinalIgnoreCase)) == true)
            return ToastView("سری ساخت تکراری است", model);

        var orderInfo = await orderInfoService.GetConfirmedByBatchNum(batchNum, token);
        if (orderInfo == null)
            return ToastView("فایل مربوط به سری ساخت مورد نظر آپلود نشده است", model);

        var availableUids = await uidInfoService.GetAvailableByBatchNum(
            batchNum, CurrentUserRecipientId ?? 0, token);

        if (!availableUids.Any())
            return ToastView("کلیه ویال‌های مربوط به سری ساخت وارد شده ورود خورده‌اند", model);

        model.NewLine.ExpiryDate = orderInfo.ExpirationDate;
        model.NewLine.ProductionDate = orderInfo.ManufactureDate;
        model.NewLine.Quantity = availableUids.Count;

        await EnsureArrivalCreatedAsync(model, token);

        var lineDto = MapNewLineToDto(model);
        await arrivalService.AddLine(model.ArrivalId!.Value, lineDto, token);

        var itemInfo = await itemService.GetById(model.NewLine.ItemId!.Value, token);
        if (itemInfo?.RequiresDiluent == true && itemInfo.DiluentId.HasValue)
        {
            await arrivalService.AddLine(model.ArrivalId.Value, new ArrivalLineDto
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
            }, token);
        }

        return RedirectToAction("New", new { id = model.ArrivalId.Value });
    }


    private async Task<IActionResult> AddHalalLine(ArrivalViewModel model, CancellationToken token)
    {
        if (!model.NewLine.ItemId.HasValue)
            return ToastView("آیتم را انتخاب کنید", model);

        if (!model.NewLine.DosePerVial.HasValue || model.NewLine.DosePerVial <= 0)
            return ToastView("مقدار دوز باید بیشتر از صفر باشد", model);

        if (!model.NewLine.Quantity.HasValue || model.NewLine.Quantity <= 0)
            return ToastView("تعداد باید بیشتر از صفر باشد", model);

        if (string.IsNullOrWhiteSpace(model.NewLine.ProductionDateShamsi))
            return ToastView("تاریخ تولید را وارد کنید", model);

        if (string.IsNullOrWhiteSpace(model.NewLine.ExpiryDateShamsi))
            return ToastView("تاریخ انقضا را وارد کنید", model);


        if (model.NewLine.ProductionDateMode == "jalali")
        {
            model.NewLine.ProductionDate = DateTimeHelper.ToMiladi(model.NewLine.ProductionDateShamsi);
        }
        else
        {
            model.NewLine.ProductionDate = DateTime.ParseExact(
                model.NewLine.ProductionDateShamsi,
                "yyyy/MM/dd",
                CultureInfo.InvariantCulture
            );
        }
        if (model.NewLine.ExpiryDateMode == "jalali")
        {
            model.NewLine.ExpiryDate = DateTimeHelper.ToMiladi(model.NewLine.ExpiryDateShamsi);
        }
        else
        {
            model.NewLine.ExpiryDate = DateTime.ParseExact(
                model.NewLine.ExpiryDateShamsi,
                "yyyy/MM/dd",
                CultureInfo.InvariantCulture
            );
        }

        model.NewLine.NumVials = model.NewLine.Quantity / model.NewLine.DosePerVial;

        model.NewLine.DosePerVialId = model.NewLine.DosePerVialId;

        await EnsureArrivalCreatedAsync(model, token);
        var lineDto = MapNewLineToDto(model);
        await arrivalService.AddLine(model.ArrivalId!.Value, lineDto, token);

        return RedirectToAction("New", new { id = model.ArrivalId.Value, tab = "halal" });
    }


    private async Task<IActionResult> SaveDraft(ArrivalViewModel model, CancellationToken token)
    {
        if (!model.ArrivalId.HasValue || model.ArrivalId == 0 ||
            !await arrivalService.DoesHaveArrivalLineByArrivalId(model.ArrivalId.Value))
        {
            TempData["ToastMessage"] = "ابتدا یک کالا اضافه کنید تا ورود ساخته شود";
            TempData["ToastType"] = "error";
            return View("New", model);
        }

        var dto = await BuildCreateDtoAsync(model, token);
        await arrivalService.SetDraft(model.ArrivalId.Value, dto, token);

        TempData["ToastMessage"] = "به عنوان پیش‌نویس ذخیره شد";
        TempData["ToastType"] = "info";
        return RedirectToAction("Index");
    }

    private async Task<IActionResult> Confirm(ArrivalViewModel model, CancellationToken token)
    {
        if (!model.ArrivalId.HasValue || model.ArrivalId == 0 ||
            !await arrivalService.DoesHaveArrivalLineByArrivalId(model.ArrivalId.Value))
        {
            TempData["ToastMessage"] = "ابتدا یک کالا اضافه کنید";
            TempData["ToastType"] = "error";
            return View("New", model);
        }

        var dto = await BuildCreateDtoAsync(model, token);
        await arrivalService.Confirm(model.ArrivalId.Value, dto, token);

        var confirmed = await arrivalService.GetById(model.ArrivalId.Value, token);

        bool isHalal = string.Equals(model.ArrivalType, "halal", StringComparison.OrdinalIgnoreCase);

        foreach (var line in confirmed?.Lines ?? new List<ArrivalLineDto>())
        {
            if (string.IsNullOrWhiteSpace(line.BatchNum)) continue; 

            await uidInfoService.SetArrivalFkForBatchNums(
                new List<string> { line.BatchNum! },
                model.ArrivalId.Value,
                line.DosePerVialId,
                line.StoreId,
                line.ItemId,
                isHalal, 
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

    private async Task<IActionResult> HandleEditLine(string command, ArrivalViewModel model, CancellationToken token)
    {
        if (!TryParseLineId(command, out var lineId))
            return ToastView("شناسه سطر معتبر نیست", model);

        if (!model.ArrivalId.HasValue || model.ArrivalId.Value == 0)
            return ToastView("شناسه ورود معتبر نیست", model);

        var dto = await arrivalService.GetById(model.ArrivalId.Value, token);
        var line = dto?.Lines.FirstOrDefault(l => l.ArrivalLineId == lineId);

        if (line == null)
            return ToastView("سطر مورد نظر یافت نشد", model);

        await uidInfoService.ClearArrivalFkByLineId(lineId, token);
        await arrivalService.CancelLine(lineId, token);
        await PopulateLookupViewBagsAsync(token);

        var updated = await arrivalService.GetById(model.ArrivalId.Value, token);
        model.Lines = await MapLinesToViewModelAsync(updated?.Lines ?? new List<ArrivalLineDto>(), token);
        model.NewLine = MapLineToViewModel(line, null, null, null);
        model.EditingLineId = lineId;

        return View("New", model);
    }

    private async Task<IActionResult> HandleDeleteLine(string command, ArrivalViewModel model, CancellationToken token)
    {
        if (!TryParseLineId(command, out var lineId))
            return ToastView("شناسه سطر معتبر نیست", model);

        if (!model.ArrivalId.HasValue || model.ArrivalId.Value == 0)
            return ToastView("شناسه ورود معتبر نیست", model);

        return await DeleteLine(lineId, model.ArrivalId.Value, token);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLine(int id, int arrivalId, CancellationToken token)
    {
        await uidInfoService.ClearArrivalFkByLineId(id, token);
        await arrivalService.CancelLine(id, token);

        TempData["ToastMessage"] = "سطر حذف شد";
        TempData["ToastType"] = "success";
        return RedirectToAction("New", new { id = arrivalId });
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
            batchNum, CurrentUserRecipientId ?? 0, token);

        if (!availableUids.Any())
            return Json(new { success = false, message = "کلیه ویال‌های مربوط به سری ساخت وارد شده ورود خورده‌اند" });

        var prodShamsi = orderInfo.ManufactureDate?.ToShamsi();
        var prodMiladi = orderInfo.ManufactureDate?.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
        var expShamsi = orderInfo.ExpirationDate?.ToShamsi();
        var expMiladi = orderInfo.ExpirationDate?.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);

        return Json(new
        {
            success = true,
            quantity = availableUids.Count,
            productionDateShamsi = prodShamsi,
            productionDateMiladi = prodMiladi,
            hasProdShamsi = !string.IsNullOrEmpty(prodShamsi),
            hasProdMiladi = !string.IsNullOrEmpty(prodMiladi),
            expiryDateShamsi = expShamsi,
            expiryDateMiladi = expMiladi,
            hasExpShamsi = !string.IsNullOrEmpty(expShamsi),
            hasExpMiladi = !string.IsNullOrEmpty(expMiladi),
            hasExpiry = orderInfo.ExpirationDate.HasValue,
            itemId = orderInfo.FkItemId,
            itemName = orderInfo.ItemName,
            dosePerVialId = orderInfo.FkDosePerVialId,
            doseName = orderInfo.DoseName,
            itemClassName = "واکسن"
        });
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
        var list = (await itemService.GetItemDoesPerVialsAsync(itemId, token))
            .OrderBy(z => z.NumDosePerVial)
            .Select(x => new { value = x.DosePerVialId, text = x.NumDosePerVial.ToString() });
        return Json(list);
    }


    [HttpGet]
    public async Task<IActionResult> GetVvmStages(int vvmTypeId, CancellationToken token)
    {
        var stages = await baseInfoService.GetByParentIdAsync(vvmTypeId, token);
        return Json(stages.Select(x => new { id = x.Id, text = x.Title }));
    }

    [HttpGet]
    public async Task<IActionResult> GetHalalItems(CancellationToken token)
    {
        var list = (await itemService.GetActives(token))
            .Where(x => x.FkClassId == BaseInfoIds.HalalItemClassId)
            .Select(x => new { id = x.ItemId, text = x.FullName });
        return Json(list);
    }

    [HttpGet]
    public async Task<IActionResult> GetHalalDoses(int itemId, CancellationToken token)
    {
        var list = (await itemService.GetItemDoesPerVialsAsync(itemId, token))
            .OrderBy(z => z.NumDosePerVial)
            .Select(x => new { value = x.DosePerVialId, text = x.NumDosePerVial.ToString() });
        return Json(list);
    }


    private async Task EnsureArrivalCreatedAsync(ArrivalViewModel model, CancellationToken token)
    {
        if (!model.ArrivalId.HasValue || model.ArrivalId == 0)
        {
            var dto = await BuildCreateDtoAsync(model, token);
            model.ArrivalId = await arrivalService.Create(dto, token);
        }
    }

    private async Task<ArrivalCreateDto> BuildCreateDtoAsync(ArrivalViewModel model, CancellationToken token)
    {
        var period = (await fiscalService.GetActives(token)).First();
        return new ArrivalCreateDto
        {
            Date = DateTimeHelper.ToMiladi(model.Date) ?? DateTime.Now,
            ReportNumber = model.ReportNumber ?? "",
            FiscalPeriodId = period.FiscalPeriodId,
            IsConfirmed = false,
            IsDrafted = true,
            IsCancelled = false,
            EnterById = CurrentUserId.ToString() ?? "",
            Note = model.Note ?? "",
            UserRecipientId = CurrentUserRecipientId ?? 0,
            ArrivalType = model.ArrivalType,
        };
    }

    private ArrivalLineDto MapNewLineToDto(ArrivalViewModel model) => new()
    {
        ArrivalId = model.ArrivalId!.Value,
        BatchNum = model.NewLine.BatchNum ?? "",
        CommercialNameId = model.NewLine.CommercialNameId,
        DosePerVialId = model.NewLine.DosePerVialId,
        ExpiryDate = model.NewLine.ExpiryDate,
        ProductionDate = model.NewLine.ProductionDate,
        ItemDetailId = model.NewLine.ItemDetailId,
        ItemId = model.NewLine.ItemId,
        ManufacturerId = model.NewLine.ManufacturerId,
        Note = model.NewLine.Note ?? "",
        NumVials = model.NewLine.NumVials,
        Quantity = model.NewLine.Quantity,
        StoreId = model.NewLine.StoreId,
        UseId = model.NewLine.UseId,
        VvmStageId = model.NewLine.VvmStageId,
        VvmTypeId = model.NewLine.VvmTypeId,
    };

    private async Task<List<ArrivalLineViewModel>> MapLinesToViewModelAsync(
        IEnumerable<ArrivalLineDto> lines, CancellationToken token)
    {
        var allItems = await itemService.GetActives(token);
        var commercialNames = await baseInfoService.GetByParentIdAsync(35, token);
        var allStores = await storeService.GetAll(token);
        var storeFiltered = allStores.Where(s => s.RecipientId == CurrentUserRecipientId).ToList();

        var result = new List<ArrivalLineViewModel>();
        foreach (var l in lines)
        {
            var doses = (l.ItemId.HasValue && !string.IsNullOrEmpty(l.BatchNum))
                ? await itemService.GetItemDoesPerVialsAsync(l.ItemId.Value, token)
                : Enumerable.Empty<dynamic>();

            result.Add(MapLineToViewModel(l, allItems, commercialNames, storeFiltered, doses));
        }

        return result;
    }

    private static ArrivalLineViewModel MapLineToViewModel(
        ArrivalLineDto l,
        IEnumerable<dynamic>? allItems,
        IEnumerable<dynamic>? commercialNames,
        IEnumerable<dynamic>? allStores,
        IEnumerable<dynamic>? doses = null) => new()
        {
            ArrivalLineId = l.ArrivalLineId,
            ArrivalId = l.ArrivalId,
            BatchNum = l.BatchNum,
            CommercialNameId = l.CommercialNameId,
            CommercialName = commercialNames?.FirstOrDefault(z => z.Id == l.CommercialNameId)?.Title,
            ItemName = allItems?.FirstOrDefault(z => z.ItemId == l.ItemId)?.FullName,
            DoseName = doses?.FirstOrDefault(z => z.DosePerVialId == l.DosePerVialId)?.NumDosePerVial.ToString(),
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
            StoreName = allStores?.FirstOrDefault(z => z.StoreId == l.StoreId)?.FullName,
            UseId = l.UseId,
            VvmStageId = l.VvmStageId,
            VvmTypeId = l.VvmTypeId,
        };

    private async Task PopulateLookupViewBagsAsync(CancellationToken ct)
    {
        ViewBag.Items = (await itemService.GetActives(ct))
            .ToSelectList(x => x.ItemId, x => x.FullName);

        ViewBag.Stores = (await storeService.GetAll(ct))
            .Where(z => z.RecipientId == CurrentUserRecipientId)
            .ToSelectList(x => x.StoreId, x => x.FullName);

        ViewBag.DosePerVials =
            (await baseInfoService.GetByParentIdAsync(BaseInfoIds.DosePerVial, ct))
            .ToSelectList(x => x.Id, x => x.Title);

        ViewBag.VvmTypes =
            (await baseInfoService.GetByParentIdAsync(BaseInfoIds.VvmTypes, ct))
            .ToSelectList(x => x.Id, x => x.Title);

        ViewBag.VvmStages =
            (await baseInfoService.GetByParentIdAsync(BaseInfoIds.VvmStages, ct))
            .ToSelectList(x => x.Id, x => x.Title);

        ViewBag.Manufacturers =
            (await baseInfoService.GetByParentIdAsync(BaseInfoIds.Manufacturers, ct))
            .ToSelectList(x => x.Id, x => x.Title);

        ViewBag.BrandNames =
            (await baseInfoService.GetByParentIdAsync(BaseInfoIds.BrandNames, ct))
            .ToSelectList(x => x.Id, x => x.Title);

        ViewBag.Uses =
            (await baseInfoService.GetByParentIdAsync(BaseInfoIds.Uses, ct))
            .ToSelectList(x => x.Id, x => x.Title);

        ViewBag.ItemTypes =
            (await baseInfoService.GetByParentIdAsync(BaseInfoIds.ItemClassesId, ct))
            .ToSelectList(x => x.Id, x => x.Title);

        ViewBag.HalalItems = (await itemService.GetActives(ct))
            .Where(x => x.FkClassId == BaseInfoIds.HalalItemClassId)
            .ToSelectList(x => x.ItemId, x => x.FullName);
    }

    private IActionResult ToastView(string message, ArrivalViewModel model)
    {
        TempData["ToastMessage"] = message;
        TempData["ToastType"] = "error";
        return View("New", model);
    }

    private static bool TryParseLineId(string command, out int lineId) =>
        int.TryParse(command.Split('_').Last(), out lineId);
}