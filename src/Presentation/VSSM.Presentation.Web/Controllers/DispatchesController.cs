using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.Dispatches;
using VSSM.Application.Helpers;
using VSSM.Application.Interfaces;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Const;
using VSSM.Presentation.Web.Extensions;
using VSSM.Presentation.Web.Models.Base;

namespace VSSM.Presentation.Web.Controllers;

    public class DispatchesController(
        IStoreService storeService,
        IRecipientService recipientService,
        IDispatchService dispatchService,
        IBaseInfoService baseInfoService,
        DispatchScanSessionService dispatchScan,
        IAuditService audit) : BaseController
{

    [HttpGet]
    public async Task<IActionResult> GetItemsByClassAsync(int classId, CancellationToken ct)
    {
        var items = await dispatchService.GetItemsByClassAsync(classId, ct);
        return Json(items.Select(i => new { i.ItemId, i.FullName }));
    }

    [HttpGet]
    public async Task<IActionResult> GetDispatchLines(int id, CancellationToken ct)
    {
        var lines = await dispatchService.GetDispatchLinesForEditAsync(id, ct);
        return Json(lines);
    }


    [HttpGet]
    public async Task<IActionResult> GetBatchesByItem(int itemId, CancellationToken ct)
    {
        var rows = await dispatchService.GetBatchesByItemAsync(itemId, CurrentUserRecipientId.GetValueOrDefault(), ct);
        return Json(rows);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateDispatchDto createDispatcheDto,
        string submitType,
        string? scanSessionId,
        CancellationToken cancellationToken)
    {
        createDispatcheDto.SenderId = CurrentUserRecipientId.GetValueOrDefault();
        createDispatcheDto.EnteredById = CurrentUserId.GetValueOrDefault();

        if (createDispatcheDto.Items.Any(i => i.DoseCount <= 0))
        {
            TempData["ToastMessage"] = "برای کلیه کالاهای ارسالی تعداد را مشخص نمایید";
            TempData["ToastType"] = "error";
            await PopulateLookupViewBagsAsync(cancellationToken);
            return View("New", createDispatcheDto);
        }

        var (dispatchId, seqPreview) = await dispatchService.CreateDispatchAsync(
            createDispatcheDto, "draft", cancellationToken);



        if (submitType == "final" && !string.IsNullOrEmpty(scanSessionId))
        {
            var session = dispatchScan.GetSession(scanSessionId);

            if (!session.Items.Any())
            {
                TempData["ToastMessage"] = "هیچ UID معتبری اسکن نشده است.";
                TempData["ToastType"] = "error";
                return RedirectToAction("Edit", new { id = dispatchId });
            }

            var mismatchItems = new List<string>();
            foreach (var item in createDispatcheDto.Items)
            {
                var scannedCount = session.Items
                    .Where(x => x.ItemId == item.ItemId && x.BatchNumber == item.BatchNumber)
                    .Sum(x => x.Count);

                if (scannedCount != item.DoseCount)
                {
                    mismatchItems.Add($"{item.ItemName} (درخواستی: {item.DoseCount}, اسکن‌شده: {scannedCount})");
                }
            }

            if (mismatchItems.Any())
            {
                TempData["ToastMessage"] = $"تعداد UIDهای اسکن‌شده با تعداد درخواستی مطابقت ندارد: {string.Join(" - ", mismatchItems)}";
                TempData["ToastType"] = "error";
                return RedirectToAction("Edit", new { id = dispatchId });
            }


            var confirmResult = await dispatchService.ConfirmDispatchUidAsync(scanSessionId, dispatchId);
            if (!confirmResult.success)
            {
                TempData["ToastMessage"] = confirmResult.message;
                TempData["ToastType"] = "error";
                return RedirectToAction("Edit", new { id = dispatchId });
            }

            await dispatchService.ConfirmDispatchAsync(dispatchId, cancellationToken);

            await dispatchService.DecrementStockOnFinalAsync(createDispatcheDto.Items, cancellationToken);

            await dispatchService.LinkUidsToDispatchUidAsync(dispatchId, cancellationToken);

            TempData["ToastMessage"] = $"پیش فاکتور با شماره {seqPreview} با موفقیت ثبت و نهایی شد.";
            TempData["ToastType"] = "success";
        }
        else
        {
            TempData["ToastMessage"] = $"پیش فاکتور با شماره {seqPreview} با موفقیت ثبت شد.";
            TempData["ToastType"] = "success";
        }

        return RedirectToAction("Index");
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(
        UpdateDispatchDto updateDispatcheDto,
        string submitType,
        string? scanSessionId,
        CancellationToken cancellationToken)
    {
        updateDispatcheDto.SenderId = CurrentUserRecipientId.Value;
        updateDispatcheDto.EnteredById = CurrentUserId.Value;

        try
        {
            if (submitType == "final")
            {
                if (string.IsNullOrEmpty(scanSessionId))
                {
                    TempData["ToastMessage"] = "برای ثبت نهایی باید حداقل یک UID اسکن شود.";
                    TempData["ToastType"] = "error";
                    await PopulateLookupViewBagsAsync(cancellationToken);
                    return View("Edit", updateDispatcheDto);
                }

                var session = dispatchScan.GetSession(scanSessionId);
                if (session == null || !session.Items.Any())
                {
                    TempData["ToastMessage"] = "هیچ UID معتبری اسکن نشده است.";
                    TempData["ToastType"] = "error";
                    await PopulateLookupViewBagsAsync(cancellationToken);
                    return View("Edit", updateDispatcheDto);
                }

                var mismatchItems = new List<string>();
                foreach (var item in updateDispatcheDto.Items)
                {
                    var scannedCount =
                         session.Items
                         .Where(x =>
                             x.ItemId == item.ItemId &&
                             x.BatchNumber == item.BatchNumber)
                         .Sum(x => x.Count);


                    if (scannedCount != item.DoseCount)
                    {
                        mismatchItems.Add(
                            $"{item.ItemName} درخواست:{item.DoseCount} اسکن:{scannedCount}");
                    }

                    if (scannedCount != item.DoseCount)
                    {
                        mismatchItems.Add($"{item.ItemName} (درخواستی: {item.DoseCount}, اسکن‌شده: {scannedCount})");
                    }
                }

                if (mismatchItems.Any())
                {
                    TempData["ToastMessage"] = $"تعداد UIDهای اسکن‌شده با تعداد درخواستی مطابقت ندارد: {string.Join(" - ", mismatchItems)}";
                    TempData["ToastType"] = "error";
                    await PopulateLookupViewBagsAsync(cancellationToken);
                    return View("Edit", updateDispatcheDto);
                }
            }

            updateDispatcheDto.IsDrafted = true;
            await dispatchService.UpdateDispatchAsync(updateDispatcheDto, submitType, cancellationToken);

            if (submitType == "final" && !string.IsNullOrEmpty(scanSessionId))
            {
                var confirmResult = await dispatchService.ConfirmDispatchUidAsync(scanSessionId, updateDispatcheDto.Id);
                if (!confirmResult.success)
                {
                    TempData["ToastMessage"] = confirmResult.message;
                    TempData["ToastType"] = "error";
                    await PopulateLookupViewBagsAsync(cancellationToken);
                    return View("Edit", updateDispatcheDto);
                }
            }

            TempData["ToastMessage"] = "عملیات با موفقیت انجام شد.";
            TempData["ToastType"] = "success";
        }
        catch (ApplicationException ex)
        {
            await PopulateLookupViewBagsAsync(cancellationToken);
            ViewBag.ErrorMessage = ex.Message;
            return View("Edit", updateDispatcheDto);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken token)
    {
        await dispatchService.RestoreStockAsync(id, token);
        await dispatchService.DeleteDispatchAsync(id, token);

        TempData["ToastMessage"] = "ارسال با موفقیت حذف شد.";
        TempData["ToastType"] = "success";
        await audit.LogAsync("Warning", $"Dispatch deleted: id={id}", User.Identity?.Name, HttpContext.Connection.RemoteIpAddress?.ToString(), HttpContext.Request.Path, HttpContext.Request.Method);

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken, string? search,
                                          int pageNumber = 1,
                                          int pageSize = 10,
                                          string? status = null,
                                          string sort = "Orders",
                                          string direction = "desc")
    {
        var (data, count) = await dispatchService.GetPagedDispatchesAsync(
            search, pageNumber, pageSize, sort, direction, status, CurrentUserRecipientId.Value, cancellationToken);

        var model = new GridViewModel
        {
            SearchTerm = search,
            CurrentSort = sort,
            CurrentDirection = direction,
            Data = new PagedResult<object>
            {
                Items = data.Cast<object>().ToList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = count
            },
            Columns = new List<GridColumn>
        {
            new() { Header = "تاریخ", PropertyName = nameof(DispatchListDto.DateText), Sortable = false },
            new() { Header = "گیرنده", PropertyName = nameof(DispatchListDto.ReceiverName), Sortable = false },
            new() { Header = "شماره فاکتور", PropertyName = nameof(DispatchListDto.DispatchVoucherNumber), Sortable = false },
            new() { Header = "پیش فاکتور", PropertyName = nameof(DispatchListDto.IsDraftedText), Sortable = false },
            new() { Header = "وضعیت گیرنده", PropertyName = nameof(DispatchListDto.ReceiverStatusText), Sortable = false },
            new() { Header = "عملیات", IsActionColumn = true }
        },
            Actions = new List<GridAction>
        {
            // ── دکمه ویرایش (فقط برای مواردی که تایید نشده‌اند) ──
            new()
            {
                Title = "ویرایش",
                Controller = "Dispatches",
                Action = "Edit",
                RouteIdProperty = nameof(DispatchListDto.Id),
                IconClass = "fa fa-edit",
                DynamicSkip = item =>
                {
                    var row = (DispatchListDto)item;
                    return (row.IsConfirmed.HasValue && row.IsConfirmed.Value).ToString();
                }
            },
            // ── دکمه مشاهده (فقط برای مواردی که تایید شده‌اند) ──
            new()
            {
                Title = "مشاهده",
                Controller = "Dispatches",
                Action = "Edit",
                RouteIdProperty = nameof(DispatchListDto.Id),
                IconClass = "fa fa-eye",
                DynamicSkip = item =>
                {
                    var row = (DispatchListDto)item;
                    return (!(row.IsConfirmed.HasValue && row.IsConfirmed.Value)).ToString();
                }
            },
            new()
            {
                Title = "چاپ پیش فاکتور",
                Controller = "Dispatches",
                Action = "Print",
                RouteIdProperty = nameof(DispatchListDto.Id),
                IconClass = "fa fa-print",
                DynamicSkip = item =>
                {
                    var row = (DispatchListDto)item;
                    if(row.IsConfirmed.HasValue && row.IsConfirmed.Value)
                            return false.ToString();
                    return (!(row.IsDrafted.HasValue && row.IsDrafted.Value)).ToString();
                }
            },
            new()
            {
                Title = "حذف",
                Controller = "Dispatches",
                Action = "Delete",
                Method = "Post",
                RouteIdProperty = nameof(DispatchListDto.Id),
                IconClass = "fa fa-trash",
                LinkClass = "text-danger",
                DynamicSkip = item =>
                {
                    var row = (DispatchListDto)item;
                    return (row.ReceiverStatus != 0).ToString();
                }
            }
        }
        };

        return View(model);
    }


    public async Task<IActionResult> Print(int id)
    {
        var dispatch = await dispatchService.GetDispatchPrintDataAsync(id);

        return View(dispatch);
    }
    public async Task<IActionResult> ToBeArrivedIndex(CancellationToken cancellationToken, string? search,
                                    int pageNumber = 1,
                                    int pageSize = 10,
                                    string sort = "Orders",
                                    string direction = "desc")
    {
        var (data, count) = await dispatchService.GetPagedToBeArrivedDispatchesAsync(search, pageNumber, pageSize, sort, direction, CurrentUserRecipientId.Value, cancellationToken);

        var model = new GridViewModel
        {
            SearchTerm = search,
            CurrentSort = sort,
            CurrentDirection = direction,
            Data = new PagedResult<object>
            {
                Items = data.Cast<object>().ToList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = count
            },
            Columns = new List<GridColumn>
            {
                new() { Header = "فرستنده", PropertyName = nameof(ToBeArrivedDispatchListDto.SenderName),  Sortable = false },
                new() { Header = "تاریخ", PropertyName = nameof(ToBeArrivedDispatchListDto.DateText), Sortable = false },
                new() { Header = "شماره درخواست", PropertyName = nameof(ToBeArrivedDispatchListDto.RequestNum),  Sortable = false },
                new() { Header = "شماره فاکتور", PropertyName = nameof(ToBeArrivedDispatchListDto.DispatchVoucherNumber),  Sortable = false },
                new() { Header = "تاریخ ورود", PropertyName = nameof(ToBeArrivedDispatchListDto.EntryDateText),  Sortable = false },
                new() { Header = "عملیات", IsActionColumn = true}
            },
            Actions = new List<GridAction>
            {
                new()
                {
                    Title = "دریافت",
                    Controller = "Dispatches",
                    Action = "Arrive",
                    RouteIdProperty = nameof(DispatchListDto.Id),
                    LinkClass = "",
                    IconClass = "fa fa-dropbox"
                }
            }
        };


        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(int id, CancellationToken token)
    {
        await dispatchService.ConfirmDispatchAsync(id, token);
        await audit.LogAsync("Information", $"Dispatch confirmed: id={id}", User.Identity?.Name, HttpContext.Connection.RemoteIpAddress?.ToString(), HttpContext.Request.Path, HttpContext.Request.Method);

        return RedirectToAction("Index");

    }


    public async Task<IActionResult> New(CancellationToken cancellationToken)
    {
        await PopulateLookupViewBagsAsync(cancellationToken);

        var reportNumber = await dispatchService.GenerateReportNumberAsync();

        var model = new CreateDispatchDto
        {
            Date = DateTime.Now.ToShamsi(),
            ReportNumber = reportNumber
        };

        return View(model);
    }


    public async Task<IActionResult> Arrive(long id, CancellationToken cancellationToken)
    {
        ViewBag.DispatchLines = await dispatchService.GetDispatchLinesAsync(id, cancellationToken);
        ViewBag.Stores = ((await storeService.GetAll(cancellationToken)).Where(store => store.RecipientId == CurrentUserRecipientId)).ToList();

        return View();
    }


    public async Task<IActionResult> ArriveSubmit(DispatchArrivedDto arrivedDto, CancellationToken cancellationToken)
    {
        await dispatchService.DispatchArrivedAsync(arrivedDto, CurrentUserId.Value, CurrentUserRecipientId.Value, cancellationToken);
        return RedirectToAction("ToBeArrivedIndex");
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var dispatch = await dispatchService.GetByIdAsync(id);
        if (dispatch == null)
            return NotFound();

        await PopulateLookupViewBagsAsync(cancellationToken);

        return View(dispatch);
    }


    [HttpPost("AddUidToSession")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddUidToSession(
        string sessionId,
        int? dispatchLineId,
        int? itemDetailStoreId,
        string barcode)
    {
        var result = await dispatchService.SetUidAsync(
            sessionId,
            dispatchLineId,
            itemDetailStoreId,
            barcode,
            CurrentUserRecipientId.GetValueOrDefault()
        );

        return Ok(new
        {
            success = result.isSuccess,
            message = result.message,
            count = result.count,
            itemId = result.itemId
        });
    }

    [HttpGet("GetScanSession")]
    public IActionResult GetScanSession(string sessionId)
    {
        var session =
            dispatchScan.GetSession(sessionId);


        return Ok(new
        {
            Success = true,
            Data = session.Items
        });
    }

    [HttpPost("RemoveScanUid")]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveScanUid(
    string sessionId,
    string uid)
    {

        var result =
            dispatchScan.RemovePackage(
                sessionId,
                uid);


        return Ok(new
        {
            Success = result
        });
    }

    private async Task PopulateLookupViewBagsAsync(CancellationToken ct)
    {
        ViewBag.TransportModes =
            (await baseInfoService.GetByParentIdAsync(BaseInfoIds.TransportModes, ct))
            .ToSelectList(x => x.Id, x => x.Title);

        ViewBag.ReceivingMethods =
            (await baseInfoService.GetByParentIdAsync(BaseInfoIds.ReceivingModes, ct))
            .ToSelectList(x => x.Id, x => x.Title);

        ViewBag.ItemTypes =
            (await baseInfoService.GetByParentIdAsync(BaseInfoIds.ItemClassesId, ct))
            .ToSelectList(x => x.Id, x => x.Title);

        ViewBag.VvmStages =
            (await baseInfoService.GetByParentIdAsync(BaseInfoIds.VvmStages, ct))
            .ToSelectList(x => x.Id, x => x.Title);

        ViewBag.Recipients = await recipientService.GetActiveRecipientChildren(CurrentUserRecipientId.GetValueOrDefault(), ct);
    }

    [HttpPost("ConfirmDispatchUids")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmDispatchUids(
       string sessionId,
       int dispatchId)
    {
        var result = await dispatchService.ConfirmDispatchUidAsync(sessionId, dispatchId);

        return Ok(new
        {
            success = result.success,
            message = result.message,
            count = result.count
        });
    }

    [HttpGet("GetSessionUids")]
    public IActionResult GetSessionUids(string sessionId)
    {
        var session = dispatchScan.GetSession(sessionId);
        return Ok(session.Items);
    }
}
