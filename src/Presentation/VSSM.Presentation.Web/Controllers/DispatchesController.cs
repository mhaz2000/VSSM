using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.Dispatches;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Const;
using VSSM.Presentation.Web.Extensions;
using VSSM.Presentation.Web.Models.Base;

namespace VSSM.Presentation.Web.Controllers;

public class DispatchesController(
    IStoreService storeService,
    IRecipientService recipientService,
    IDispatchService dispatchService,
    IBaseInfoService baseInfoService) : BaseController
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
        var rows = await dispatchService.GetBatchesByItemAsync(itemId, ct);
        return Json(rows);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateDispatchDto createDispatcheDto,
        string submitType,
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
        CancellationToken cancellationToken)
    {
        updateDispatcheDto.SenderId = CurrentUserRecipientId.Value;
        updateDispatcheDto.EnteredById = CurrentUserId.Value;

        try
        {
            updateDispatcheDto.IsDrafted = true;
            await dispatchService.UpdateDispatchAsync(updateDispatcheDto, submitType, cancellationToken);

            TempData["ToastMessage"] = "عملیات با موفقیت شد";
            TempData["ToastType"] = "success";

        }
        catch (ApplicationException ex)
        {
            await PopulateLookupViewBagsAsync(cancellationToken);

            ViewBag.ErrorMessage = ex.Message;

            await PopulateLookupViewBagsAsync(cancellationToken);

            return View("Edit", updateDispatcheDto);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("SetUid")]
    public async Task<IActionResult> SetUid(int dispatchLineId, int id, string uid)
    {
        var response = await dispatchService.SetUidAsync(dispatchLineId, id, uid);
        return Ok(new { Success = response.isSuccess, Message = response.message, Count = response.count, RowId = response.itemId });
    }

    [HttpPost("ValidateUid")]
    public async Task<IActionResult> ValidateUid(int itemDetailStoreId, string uid)
    {
        var result = await dispatchService.ValidateUidAsync(itemDetailStoreId, uid);
        return Ok(new { isSuccess = result.isSuccess, message = result.message });
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

        ViewBag.Recipients = await recipientService.GetActiveRecipients(ct);
    }


}
