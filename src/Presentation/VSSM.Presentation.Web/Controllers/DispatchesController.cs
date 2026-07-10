using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.Dispatches;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Application.Services.BaseEntities;
using VSSM.Presentation.Web.Const;
using VSSM.Presentation.Web.Extensions;
using VSSM.Presentation.Web.Models.Base;

namespace VSSM.Presentation.Web.Controllers;

public class DispatchesController(
    IStoreService storeService,
    IRecipientService recipientService,
    IDispatchService dispatchService,
    IBaseInfoService baseInfoService,
    DispatchScanSessionService dispatchScan ) : BaseController
{

    [HttpGet]
    public async Task<IActionResult> GetItemsByClassAsync(int classId, CancellationToken ct)
    {
        var items = await dispatchService.GetItemsByClassAsync(classId, ct);
        return Json(items.Select(i => new { i.ItemId, i.FullName }));
    }

    [HttpGet]
    public async Task<IActionResult> GetBatchesByItem(int itemId, CancellationToken ct)
    {
        var rows = await dispatchService.GetBatchesByItemAsync(itemId, ct);
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
        createDispatcheDto.SenderId = CurrentUserRecipientId.Value;
        createDispatcheDto.EnteredById = CurrentUserId.Value;

        if (createDispatcheDto.Items.Any(i => i.DoseCount <= 0))
        {
            TempData["ToastMessage"] = "برای کلیه کالاهای ارسالی تعداد را مشخص نمایید";
            TempData["ToastType"] = "error";
            await PopulateLookupViewBagsAsync(cancellationToken);
            return View(createDispatcheDto);
        }

        var (dispatchId, seqPreview) = await dispatchService.CreateDispatchAsync(
            createDispatcheDto, submitType, cancellationToken);

        if (submitType == "final" && !string.IsNullOrEmpty(scanSessionId))
        {
            var confirmResult = await dispatchService.ConfirmDispatchUidAsync(scanSessionId, dispatchId);
            if (!confirmResult.success)
            {
                await dispatchService.DeleteDispatchAsync(dispatchId, cancellationToken);
                TempData["ToastMessage"] = confirmResult.message;
                TempData["ToastType"] = "error";
                await PopulateLookupViewBagsAsync(cancellationToken);
                return View(createDispatcheDto);
            }
        }

        if (submitType == "final")
        {
            await dispatchService.DecrementStockOnFinalAsync(createDispatcheDto.Items, cancellationToken);
            await dispatchService.LinkUidsToDispatchUidAsync(dispatchId, cancellationToken);
        }

        TempData["ToastMessage"] = $"پیش فاکتور با شماره {seqPreview} با موفقیت ثبت شد.";
        TempData["ToastType"] = "success";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken token)
    {
        await dispatchService.RestoreStockAsync(id, token);

        await dispatchService.DeleteDispatchAsync(id, token);
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken, string? search,
                                            int pageNumber = 1,
                                            int pageSize = 10,
                                            string? status = null,
                                            string sort = "Orders",
                                            string direction = "desc")
    {

        var (data, count) = await dispatchService.GetPagedDispatchesAsync(search, pageNumber, pageSize, sort, direction, status, CurrentUserRecipientId.Value, cancellationToken);

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
                new() { Header = "تاریخ", PropertyName = nameof(DispatchListDto.DateText),  Sortable = false },
                new() { Header = "شماره درخواست", PropertyName = nameof(DispatchListDto.RequestNum), Sortable = false },
                new() { Header = "شماره فاکتور", PropertyName = nameof(DispatchListDto.DispatchVoucherNumber), Sortable = false },
                new() { Header = "پیش فاکتور", PropertyName = nameof(DispatchListDto.IsDraftedText),  Sortable = false },
                new() { Header = "وضعیت گیرنده", PropertyName = nameof(DispatchListDto.ReceiverStatusText),  Sortable = false },
                new() { Header = "عملیات", IsActionColumn = true}
            },
            Actions = new List<GridAction>
            {
                new()
                {
                    Title = "ویرایش",
                    Controller = "Dispatches",
                    Action = "Edit",
                    RouteIdProperty = nameof(DispatchListDto.Id),
                    LinkClass = "",
                    IconClass = "fa fa-edit",
                    DynamicSkip = item =>
                    {
                        var row = (DispatchListDto)item;

                        return (row.IsConfirmed.HasValue && row.IsConfirmed.Value).ToString();
                    }

                },
                new()
                {
                    Title = "چاپ پیش فاکتور",
                    Controller = "Dispatches",
                    Action = "Print",
                    RouteIdProperty = nameof(DispatchListDto.Id),
                    LinkClass = "",
                    IconClass = "fa fa-print",
                    DynamicSkip = item =>
                    {
                        var row = (DispatchListDto)item;

                        return (!(row.IsDrafted.HasValue && row.IsDrafted.Value)).ToString();
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

    public async Task<IActionResult> Confirm(int id, CancellationToken token)
    {
        await dispatchService.ConfirmDispatchAsync(id, token);

        return RedirectToAction("Index");

    }


    public async Task<IActionResult> New(CancellationToken cancellationToken)
    {

        await PopulateLookupViewBagsAsync(cancellationToken);

        return View();
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
                    return View(updateDispatcheDto);
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

    [HttpPost("AddUidToSession")]
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
            barcode
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

        ViewBag.Recipients = await recipientService.GetActiveRecipientChildren(CurrentUserRecipientId.GetValueOrDefault(),ct);
    }

    [HttpPost("ConfirmDispatchUids")]
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
