using Microsoft.AspNetCore.Mvc;
using VSSM.Application.Common.Helpers;
using VSSM.Application.DTOs.BaseEntities;
using VSSM.Application.DTOs.BaseEntities.Dispatches;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Application.Services.BaseEntities;
using VSSM.Domain.Enums;
using VSSM.Presentation.Web.Models.Base;

namespace VSSM.Presentation.Web.Controllers;

public class DispatchesController(
    IManufacturerService manufacturerService,
    IVvmstageService vvmstageService,
    IVvmtypeService vvmtypeService,
    IUseService useService,
    IStoreService storeService,
    IRecipientService recipientService,
    IFiscalPeriodService fiscalPeriodService,
    IDosePerVialService dosePerVialService,
    IItemService itemService,
    IDispatchService dispatchService,
    IItemClassService itemClassService) : BaseController
{
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
                    IconClass = "fa fa-edit"
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
            await _dispatchService.ConfirmDispatchAsync(id, token);

            return RedirectToAction("Index");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken token)
        {
            await _dispatchService.DeleteDispatchAsync(id, token);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> New(CancellationToken cancellationToken)
        {

        await PopulateViewBagData(cancellationToken);

        return View();
    }

    public async Task<IActionResult> Arrive(long id, CancellationToken cancellationToken)
    {
        ViewBag.DispatchLines = await dispatchService.GetDispatchLinesAsync(id, cancellationToken);
        ViewBag.Stores = ((await storeService.GetAll(cancellationToken)).Where(store => store.RecipientId == CurrentUserRecipientId)).ToList();

        return View();
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


            var result = await _dispatchService.CreateDispatchAsync(createDispatcheDto, submitType, cancellationToken);

            TempData["ToastMessage"] = $"پیش فاکتور با شماره {result} با موفقیت ثبت شد.";
            TempData["ToastType"] = "success";

            return RedirectToAction(nameof(Index));
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

        await PopulateViewBagData(cancellationToken);

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
            await PopulateViewBagData(cancellationToken);

            ViewBag.ErrorMessage = ex.Message;

            await PopulateViewBagData(cancellationToken);

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


    private async Task PopulateViewBagData(CancellationToken cancellationToken)
    {
        ViewBag.TransportModes = Enum.GetValues(typeof(TransportMode))
            .Cast<TransportMode>()
            .Select(mode => new EnumListDto
            {
                Name = mode.ToString(),
                Description = EnumHelper.GetEnumDescription(mode),
                Value = (int)mode
            }).ToList();

        ViewBag.ReceivingModes = Enum.GetValues(typeof(ReceivingMode))
            .Cast<ReceivingMode>()
            .Select(mode => new EnumListDto
            {
                Name = mode.ToString(),
                Description = EnumHelper.GetEnumDescription(mode),
                Value = (int)mode
            }).ToList();

        ViewBag.Recipients = await recipientService.GetActiveRecipients(cancellationToken);
        ViewBag.ItemClasses = await itemClassService.GetActives(cancellationToken);
        ViewBag.VVMStages = await vvmstageService.GetActives(cancellationToken);
    }
}
