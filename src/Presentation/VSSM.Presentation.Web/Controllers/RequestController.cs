using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.RequestEntities;
using VSSM.Application.Interfaces;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Application.Interfaces.RequestEntities;
using VSSM.Application.Security;
using VSSM.Domain.Enums;
using VSSM.Infrastructure.Data.Models;
using VSSM.Presentation.Web.Const;
using VSSM.Presentation.Web.Extensions;
using VSSM.Presentation.Web.Models.Base;
using VSSM.Presentation.Web.Models.Request;

namespace VSSM.Presentation.Web.Controllers;

public class RequestController(
    IRequestService requestService,
    IBaseInfoService baseInfoService,
    IItemService itemService,
    IFiscalPeriodService fiscalPeriodService,
    IResourceAccessPolicy<TblRequest> requestAccessPolicy) : BaseController
{
    private const string ToastMessageKey = "ToastMessage";
    private const string ToastTypeKey = "ToastType";
    private const string ToastSuccess = "success";
    private const string ToastError = "error";
    private const string ToastWarning = "warning";
    private const string SuccessMessage = "عملیات با موفقیت انجام شد";
    private const string DeleteLineMarker = "deleteLine";
    private const string DeleteLinePrefix = "deleteLine_";

    private static readonly Dictionary<string, (string Text, string CssClass)> StatusBadgeMap = new()
    {
        ["Label"] = ("در حال ثبت", "badge-warning"),
        ["Draft"] = ("پیش‌نویس", "badge-secondary"),
        ["Sent"] = ("ارسال شده", "badge-info"),
        ["Confirmed"] = ("تایید شده", "badge-success"),
        ["Rejected"] = ("رد شده", "badge-danger"),
    };

    public async Task<IActionResult> Index(
        CancellationToken cancellationToken,
        string? search = null,
        int page = 1,
        int pageSize = 10,
        string sort = "Orders",
        string direction = "desc")
    {
        var paged = await requestService.GetPagedList(
            search, page, pageSize, sort, direction,
            CurrentUserRecipientId.GetValueOrDefault(), cancellationToken);

        ViewBag.CanAddRequest = CurrentUserLevel != 1;

        var model = new GridViewModel
        {
            SearchTerm = search,
            CurrentSort = sort,
            CurrentDirection = direction,
            Data = new PagedResult<object>
            {
                Items = paged.Items.Cast<object>().ToList(),
                PageNumber = paged.Paging.Page,
                PageSize = pageSize,
                TotalCount = paged.Paging.TotalPages * pageSize
            },
            Columns =
            [
                new() { Header = "شماره درخواست", PropertyName = nameof(RequestListDto.RequestNumber),     Sortable = true },
                new() { Header = "تاریخ",         PropertyName = nameof(RequestListDto.SendDateShamsi),    Sortable = true },
                new()
                {
                    Header        = "وضعیت",
                    PropertyName  = nameof(RequestListDto.Status),
                    Sortable      = true,
                    IsBadgeColumn = true,
                    BadgeMap      = StatusBadgeMap
                },
                new() { Header = "ثبت شده توسط",  PropertyName = nameof(RequestListDto.EnteredByName),     Sortable = false },
                new() { Header = "تاریخ تایید",   PropertyName = nameof(RequestListDto.ConfirmDateShamsi), Sortable = true },
                new() { Header = "عملیات",        IsActionColumn = true }
            ],
            Actions =
            [
                new()
                {
                    Title           = "مشاهده",
                    Controller      = "Request",
                    Action          = "View",
                    RouteIdProperty = nameof(RequestListDto.Id),
                    IconClass       = "fa fa-eye",
                    LinkClass       = "btn-outline-primary"
                },
                new()
                {
                    Title           = "حذف",
                    Controller      = "Request",
                    Action          = "DeleteRequest",
                    RouteIdProperty = nameof(RequestListDto.Id),
                    IconClass       = "fa fa-trash",
                    LinkClass       = "btn-outline-danger",
                    DynamicSkip     = item =>
                    {
                        var prop = item.GetType().GetProperty(nameof(RequestListDto.RequestStatus));
                        var val  = prop?.GetValue(item)?.ToString();
                        return (val == ((int)RequestStatusEnum.Confirmed).ToString()).ToString().ToLowerInvariant();
                    }
                }
            ]
        };

        return View(model);
    }

    public async Task<IActionResult> New(int? id, CancellationToken token)
    {
        if (id is null or 0)
        {
            if (CurrentUserLevel == 1)
            {
                SetToast("عدم امکان ثبت درخواست برای این سطح کاربری", ToastWarning);
                return RedirectToAction("Index");
            }

            await PopulateLookupViewBagsAsync(token);

            return View(new RequestViewModel
            {
                RequestLines = [],
                NewLine = new RequestLinesViewModel()
            });
        }

        if (!await CanAccessAsync(id.Value, ResourceOperation.Read, token))
        {
            return Forbid();
        }

        var dto = await requestService.GetById(id.Value, token);
        if (dto is null)
        {
            return NotFound();
        }

        await PopulateLookupViewBagsAsync(token);

        var model = new RequestViewModel
        {
            ConfirmDate = dto.ConfirmDate,
            EnteredBy = dto.EnteredBy,
            Id = dto.Id,
            RequestNumber = dto.RequestNumber,
            RequestStatus = dto.RequestStatus,
            SendDate = dto.SendDate,
            RequestLines = dto.RequestLines.Select(r => new RequestLinesViewModel
            {
                Id = r.Id,
                RequestId = r.RequestId,
                ArivCount = r.ArriveCount,
                ConfirmCount = r.ConfirmCount,
                CurrentCount = r.CurrentCount,
                DosePerVialId = r.DosePerVialId,
                ItemClassId = r.ItemClassId,
                ItemId = r.ItemId,
                ItemName = r.ItemName,
                Note = r.Note,
                ReqCount = r.ReqCount,
                UsedCount = r.UsedCount
            }).ToList(),
            NewLine = new RequestLinesViewModel()
        };

        return View(model);
    }

    public async Task<IActionResult> View(int id, CancellationToken token)
    {
        if (!await CanAccessAsync(id, ResourceOperation.Read, token))
        {
            return Forbid();
        }

        var dto = await requestService.GetById(id, token);
        if (dto is null)
        {
            return NotFound();
        }

        var model = new RequestViewModel
        {
            ConfirmDate = dto.ConfirmDate,
            EnteredBy = dto.EnteredBy,
            Id = dto.Id,
            RequestNumber = dto.RequestNumber,
            RequestStatus = dto.RequestStatus,
            SendDate = dto.SendDate,
            RequestLines = dto.RequestLines.Select(r => new RequestLinesViewModel
            {
                Id = r.Id,
                RequestId = r.RequestId,
                ArivCount = r.ArriveCount,
                ConfirmCount = r.ConfirmCount,
                CurrentCount = r.CurrentCount,
                DosePerVialId = r.DosePerVialId,
                ItemClassId = r.ItemClassId,
                ItemId = r.ItemId,
                ItemName = r.ItemName,
                Note = r.Note,
                ReqCount = r.ReqCount,
                UsedCount = r.UsedCount
            }).ToList(),
            NewLine = new RequestLinesViewModel()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RequestViewModel model, string command, CancellationToken token)
    {
        await PopulateLookupViewBagsAsync(token);


        if (model.Id.HasValue)
        {
            var operation = command == "StatusDelete"
                ? ResourceOperation.Delete
                : ResourceOperation.Edit;

            if (!await CanAccessAsync(model.Id.Value, operation, token))
            {
                return Forbid();
            }

            var existing = await requestService.GetById(model.Id.Value, token);
            if (existing is null)
            {
                return RedirectToAction("New");
            }

            if (existing.RequestStatus == (int)RequestStatusEnum.Confirmed)
            {
                SetToast("این درخواست تایید شده است و امکان انجام عملیات روی آن وجود ندارد", ToastError);
                return RedirectToAction("New", new { id = model.Id.Value });
            }

            if (command == "StatusSent")
            {
                await requestService.ChangeStatus(model.Id.Value, RequestStatusEnum.Sent, string.Empty, token);
                SetSuccessToast();
                return RedirectToAction("Index");
            }

            if (command == "StatusDraft")
            {
                await requestService.ChangeStatus(model.Id.Value, RequestStatusEnum.Draft, string.Empty, token);
                SetSuccessToast();
                return RedirectToAction("Index");
            }

            if (command == "StatusDelete")
            {
                await DeleteRequestCore(model.Id.Value, token);
                SetSuccessToast();
                return RedirectToAction("Index");
            }

            if (command.Contains(DeleteLineMarker, StringComparison.Ordinal))
            {
                var rawLineId = command.Replace(DeleteLinePrefix, string.Empty, StringComparison.Ordinal);
                if (int.TryParse(rawLineId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var lineId))
                {
                    await DeleteLineCore(lineId, model.Id.GetValueOrDefault(), token);
                    SetSuccessToast();
                    return RedirectToAction("Index");
                }
            }
        }

        if (!ModelState.IsValid)
        {
            await PopulateLookupViewBagsAsync(token);
            return View("New", model);
        }

        return await AddLine(model, token);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRequest(int id, CancellationToken token)
    {
        if (!await CanAccessAsync(id, ResourceOperation.Delete, token))
        {
            return Forbid();
        }

        var deleted = await DeleteRequestCore(id, token);
        if (deleted)
        {
            SetToast("عملیات حذف با موفقیت انجام شد", ToastSuccess);
            return RedirectToAction("Index");
        }

        SetToast("خطایی اتفاق افتاده است", ToastError);
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLine(int id, int requestId, CancellationToken token)
    {
        if (!await CanAccessAsync(requestId, ResourceOperation.Edit, token))
        {
            return Forbid();
        }

        await DeleteLineCore(id, requestId, token);

        SetToast("سطر حذف شد", ToastSuccess);

        return RedirectToAction("New", new { id = requestId });
    }

    [HttpGet]
    public async Task<IActionResult> GetItemsByClassAsync(int classId)
    {
        var list = await itemService.GetItemsByClassAsync(classId);

        return Json(list.Select(x => new
        {
            id = x.ItemId,
            text = x.FullName
        }));
    }

    [HttpGet]
    public async Task<IActionResult> GetItemDetailsByItem(int itemId, CancellationToken token)
    {
        var availability = await requestService.GetItemAvailabilityAsync(
            itemId, CurrentUserRecipientId.GetValueOrDefault(), token);

        if (availability.CurrentCount <= 0)
        {
            SetToast("موجودی این کالا صفر است و امکان ثبت درخواست برای آن وجود ندارد.", ToastError);
        }

        return Json(new
        {
            arivCount = availability.ArivCount,
            currentCount = availability.CurrentCount,
            usedCount = availability.UsedCount
        });
    }

    public async Task<IActionResult> ReceivedRequests(
        CancellationToken cancellationToken,
        string? search = null,
        int page = 1,
        int pageSize = 10,
        string sort = "Orders",
        string direction = "desc")
    {
        var paged = await requestService.GetReceivedRequestsPagedList(
            search,
            page,
            pageSize,
            sort,
            direction,
            CurrentUserRecipientParentId.GetValueOrDefault(),
            CurrentUserRecipientId.GetValueOrDefault(),
            cancellationToken);

        var model = new GridViewModel
        {
            SearchTerm = search,
            CurrentSort = sort,
            CurrentDirection = direction,
            Data = new PagedResult<object>
            {
                Items = paged.Items.Cast<object>().ToList(),
                PageNumber = paged.Paging.Page,
                PageSize = pageSize,
                TotalCount = paged.Paging.TotalPages * pageSize
            },
            Columns =
            [
                new() { Header = "شماره درخواست", PropertyName = nameof(RequestListDto.RequestNumber),      Sortable = true },
                new() { Header = "تاریخ",         PropertyName = nameof(RequestListDto.SendDateShamsi),     Sortable = true },
                new()
                {
                    Header        = "وضعیت",
                    PropertyName  = nameof(RequestListDto.Status),
                    Sortable      = true,
                    IsBadgeColumn = true,
                    BadgeMap      = StatusBadgeMap
                },
                new() { Header = "ثبت شده توسط",  PropertyName = nameof(RequestListDto.EnteredByName),      Sortable = true },
                new() { Header = "تاریخ تایید",   PropertyName = nameof(RequestListDto.ConfirmDateShamsi),  Sortable = true },
                new() { Header = "عملیات",        IsActionColumn = true }
            ],
            Actions =
            [
                new()
                {
                    Title           = "مشاهده",
                    Controller      = "Request",
                    Action          = "ReceivedRequestLines",
                    RouteIdProperty = nameof(RequestListDto.Id),
                    IconClass       = "fa fa-eye",
                    LinkClass       = "btn-outline-primary"
                }
            ]
        };

        return View(model);
    }

    public async Task<IActionResult> ReceivedRequestLines(int? id, CancellationToken token)
    {
        if (id is null or 0)
        {
            return NotFound();
        }

        if (!await CanAccessAsync(id.Value, ResourceOperation.Read, token))
        {
            return Forbid();
        }

        var dto = await requestService.GetById(id.Value, token);
        if (dto is null)
        {
            return NotFound();
        }

        var model = new RequestViewModel
        {
            ConfirmDate = dto.ConfirmDate,
            EnteredBy = dto.EnteredBy,
            Id = dto.Id,
            RequestNumber = dto.RequestNumber,
            RequestStatus = dto.RequestStatus,
            SendDate = dto.SendDate,
            RequestLines = dto.RequestLines.Select(r => new RequestLinesViewModel
            {
                Id = r.Id,
                RequestId = r.RequestId,
                ArivCount = r.ArriveCount,
                ConfirmCount = r.ConfirmCount,
                CurrentCount = r.CurrentCount,
                DosePerVialId = r.DosePerVialId,
                ItemClassId = r.ItemClassId,
                ItemId = r.ItemId,
                ItemName = r.ItemName,
                Note = r.Note,
                ReqCount = r.ReqCount,
                UsedCount = r.UsedCount
            }).ToList(),
            NewLine = new RequestLinesViewModel()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmReceivedRequest(int id, CancellationToken token)
    {
        if (!await CanAccessAsync(id, ResourceOperation.Confirm, token))
        {
            return Forbid();
        }

        var existing = await requestService.GetById(id, token);
        if (existing is null)
        {
            return NotFound();
        }

        if (existing.RequestStatus != (int)RequestStatusEnum.Sent)
        {
            SetToast("فقط درخواست‌های ارسال‌شده قابل تایید هستند", ToastError);
            return RedirectToAction("ReceivedRequestLines", new { id });
        }

        await requestService.ChangeStatus(id, RequestStatusEnum.Confirmed, string.Empty, token);

        SetToast("درخواست تایید شد", ToastSuccess);
        return RedirectToAction("ReceivedRequests");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectReceivedRequest(int id, CancellationToken token)
    {
        if (!await CanAccessAsync(id, ResourceOperation.Reject, token))
        {
            return Forbid();
        }

        var existing = await requestService.GetById(id, token);
        if (existing is null)
        {
            return NotFound();
        }

        if (existing.RequestStatus != (int)RequestStatusEnum.Sent)
        {
            SetToast("فقط درخواست‌های ارسال‌شده قابل رد هستند", ToastError);
            return RedirectToAction("ReceivedRequestLines", new { id });
        }

        await requestService.ChangeStatus(id, RequestStatusEnum.Rejected, string.Empty, token);

        SetToast("درخواست رد شد", ToastSuccess);
        return RedirectToAction("ReceivedRequests");
    }



    private async Task<IActionResult> AddLine(
        RequestViewModel model,
        CancellationToken token)
    {
        if (model.NewLine.ItemClassId is null or <= 0)
        {
            SetToast("انتخاب نوع کالا الزامی است.", ToastError);
            return RedirectToAction("New", new { id = model.Id });
        }

        if (model.NewLine.ItemId is null or <= 0)
        {
            SetToast("انتخاب کالا الزامی است.", ToastError);
            return RedirectToAction("New", new { id = model.Id });
        }

        if (!TryValidateModel(model.NewLine, nameof(model.NewLine)))
        {
            await PopulateLookupViewBagsAsync(token);
            return View("New", model);
        }

        if (!model.Id.HasValue || model.Id.Value <= 0)
        {
            var activeFiscalPeriod =
                (await fiscalPeriodService.GetActives(token)).FirstOrDefault();

            var createDto = new RequestCreateDto
            {
                Status = RequestStatusEnum.Label,
                SenderId = CurrentUserRecipientId,
                EnteredBy = CurrentUserId,
                ConfirmDate = null,
                RequestNumber = string.Empty,
                ReceiverId = CurrentUserRecipientParentId,
                SendDate = DateOnly.FromDateTime(DateTime.Now),
                FiscalPeriodId = activeFiscalPeriod?.FiscalPeriodId,
                Lines = []
            };

            var newId = await requestService.Create(createDto, token);
            model.Id = newId;
        }

        var lineDto = new RequestLineDto
        {
            ArriveCount = model.NewLine.ArivCount,
            ReqCount = model.NewLine.ReqCount,
            CurrentCount = model.NewLine.CurrentCount,
            UsedCount = model.NewLine.UsedCount,
            ItemClassId = model.NewLine.ItemClassId,
            ItemId = model.NewLine.ItemId,
            ConfirmCount = null,
            DosePerVialId = model.NewLine.DosePerVialId,
            Note = model.NewLine.Note,
            RequestId = model.Id
        };

        await requestService.AddLine(model.Id.Value, lineDto, token);

        SetToast("کالا اضافه شد.", ToastSuccess);

        return RedirectToAction("New", new { id = model.Id });
    }



    private async Task<bool> DeleteRequestCore(int id, CancellationToken token)
    {
        if (id <= 0)
        {
            return false;
        }

        var existing = await requestService.GetById(id, token);
        if (existing is null || existing.RequestStatus == (int)RequestStatusEnum.Confirmed)
        {
            return false;
        }

        await requestService.Delete(id);
        return true;
    }

    private async Task DeleteLineCore(int lineId, int requestId, CancellationToken token)
    {
        await requestService.DeleteLine(lineId, token);
    }

    private async Task<bool> CanAccessAsync(int id, ResourceOperation operation, CancellationToken token)
    {
        var entity = await requestService.GetEntityById(id, token);

        return entity is not null && await requestAccessPolicy.CanAsync(entity, operation, User);
    }

    private async Task PopulateLookupViewBagsAsync(CancellationToken token)
    {
        ViewBag.ItemClassList = (await baseInfoService.GetByParentIdAsync(BaseInfoIds.ItemClassesId, token))
            .ToSelectList(x => x.Id, x => x.Title);
    }

    private void SetToast(string message, string type)
    {
        TempData[ToastMessageKey] = message;
        TempData[ToastTypeKey] = type;
    }

    private void SetSuccessToast(string message = SuccessMessage) => SetToast(message, ToastSuccess);
}