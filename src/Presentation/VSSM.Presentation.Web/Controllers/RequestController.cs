using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.RequestEntities;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Application.Interfaces.RequestEntities;
using VSSM.Domain.Enums;
using VSSM.Presentation.Web.Const;
using VSSM.Presentation.Web.Extensions;
using VSSM.Presentation.Web.Models.Base;
using VSSM.Presentation.Web.Models.Request;

namespace VSSM.Presentation.Web.Controllers;

public class RequestController(
    IRequestService requestService,
    IBaseInfoService baseInfoService,
    IItemService itemService,
    IFiscalPeriodService fiscalPeriodService) : BaseController
{
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
                new() { Header = "شماره درخواست", PropertyName = nameof(RequestListDto.RequestNumber), Sortable = true  },
                new() { Header = "تاریخ",       PropertyName = nameof(RequestListDto.SendDateShamsi),    Sortable = true },
                new() { Header = "وضعیت",          PropertyName = nameof(RequestListDto.Status),        Sortable = true,
                    IsBadgeColumn = true,
                    BadgeMap = new Dictionary<string, (string Text, string CssClass)>
                    {
                        ["Label"]     = ("در حال ثبت",  "badge-warning"),
                        ["Draft"]     = ("پیش‌نویس",    "badge-secondary"),
                        ["Sent"]      = ("ارسال شده",   "badge-info"),
                        ["Confirmed"] = ("تایید شده",   "badge-success"),
                        ["Rejected"]  = ("رد شده",      "badge-danger"),

                    }},
                new() { Header = "ثبت شده توسط",  PropertyName = nameof(RequestListDto.EnteredByName), Sortable = false },
                new() { Header = "تاریخ تایید", PropertyName = nameof(RequestListDto.ConfirmDateShamsi), Sortable = true },
                new() { Header = "عملیات", IsActionColumn = true }
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
                    LinkClass       = "btn-outline-primary",
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
                        return (val == ((int)RequestStatusEnum.Confirmed).ToString()).ToString().ToLower();
                    }
                }
            ]
        };

        return View(model);
    }

    public async Task<IActionResult> New(int? id, CancellationToken token)
    {
        if (id == null || id == 0)
        {
            if (CurrentUserLevel == 1)
            {
                TempData["ToastMessage"] = "عدم امکان ثبت درخواست برای این سطح کاربری";
                TempData["ToastType"] = "warning";
                return RedirectToAction("Index");
            }
            var vm = new RequestViewModel
            {
                RequestLines = new List<RequestLinesViewModel>(),
                NewLine = new RequestLinesViewModel()
            };
            await PopulateLookupViewBagsAsync(token);
            return View(vm);
        }
        var dto = await requestService.GetById(id.Value, token);
        if (dto == null) return NotFound();
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
        var dto = await requestService.GetById(id, token);
        if (dto == null) return NotFound();

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

    public async Task<IActionResult> Create(RequestViewModel model, string command, CancellationToken token)
    {
        await PopulateLookupViewBagsAsync(token);

        if (model.Id.HasValue)
        {
            var existing = await requestService.GetById(model.Id.Value, token);
            if (existing == null) return RedirectToAction("New");

            if (existing.RequestStatus == (int)RequestStatusEnum.Confirmed)
            {
                TempData["ToastMessage"] = "این درخواست تایید شده است و امکان انجام عملیات روی آن وجود ندارد";
                TempData["ToastType"] = "error";
                return RedirectToAction("New", new { id = model.Id.Value });
            }

            if (command == "StatusSent")
            {
                await requestService.ChangeStatus(model.Id.Value, RequestStatusEnum.Sent, "", token);
                TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
                TempData["ToastType"] = "success";
                return RedirectToAction("Index");
            }
            if (command == "StatusDraft")
            {
                await requestService.ChangeStatus(model.Id.Value, RequestStatusEnum.Draft, "", token);
                TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
                TempData["ToastType"] = "success";
                return RedirectToAction("Index");
            }
            if (command == "StatusDelete")
            {
                await DeleteRequest(model.Id.Value, token);
                TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
                TempData["ToastType"] = "success";
                return RedirectToAction("Index");
            }
            if (command.Contains("deleteLine"))
            {
                var lineId = command.Replace("deleteLine_", "");
                await DeleteLine(Convert.ToInt32(lineId), model.Id.GetValueOrDefault(), token);
                TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
                TempData["ToastType"] = "success";
                return RedirectToAction("Index");
            }
        }

        return await AddLine(model, token);
    }

    private async Task<IActionResult> AddLine(RequestViewModel model, CancellationToken token)
    {
        if (!model.Id.HasValue || model.Id == 0)
        {
            if (model.NewLine.ItemClassId == null || model.NewLine.ItemClassId == 0)
            {
                TempData["ToastMessage"] = "انتخاب نوع کالا اجباری است";
                TempData["ToastType"] = "error";
                return RedirectToAction("New", new { id = model.Id });
            }
            if (model.NewLine.ItemId == null || model.NewLine.ItemId == 0)
            {
                TempData["ToastMessage"] = "انتخاب کالا اجباری است";
                TempData["ToastType"] = "error";
                return RedirectToAction("New", new { id = model.Id });
            }

            var activeFiscalPeriod = (await fiscalPeriodService.GetActives(token)).FirstOrDefault();

            var createDto = new RequestCreateDto
            {
                Status = RequestStatusEnum.Label,
                SenderId = CurrentUserRecipientId,
                EnteredBy = CurrentUserId,
                ConfirmDate = null,
                RequestNumber = "", // شماره واقعی داخل RequestService.Create ساخته می‌شود (سال-سکوئنس)
                ReceiverId = CurrentUserRecipientId,
                SendDate = DateOnly.FromDateTime(DateTime.Now),
                FiscalPeriodId = activeFiscalPeriod?.FiscalPeriodId,
                Lines = new List<RequestLineDto>()
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
            RequestId = model.Id,
        };

        await requestService.AddLine(model.Id.Value, lineDto, token);

        TempData["ToastMessage"] = "کالا اضافه شد";
        TempData["ToastType"] = "success";

        return RedirectToAction("New", new { id = model.Id });
    }
    public async Task<IActionResult> DeleteRequest(int id, CancellationToken token)
    {
        if (id > 0)
        {
            var existing = await requestService.GetById(id, token);
            if (existing != null && existing.RequestStatus != (int)RequestStatusEnum.Confirmed)
            {
                await requestService.Delete(id);
                TempData["ToastMessage"] = "عملیات حذف با موفقیت انجام شد";
                TempData["ToastType"] = "success";
                return RedirectToAction("Index");
            }
        }

        TempData["ToastMessage"] = "خطایی اتفاق افتاده است";
        TempData["ToastType"] = "error";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLine(int id, int requestId, CancellationToken token)
    {
        await requestService.DeleteLine(id, token);

        TempData["ToastMessage"] = "سطر حذف شد";
        TempData["ToastType"] = "success";

        return RedirectToAction("New", new { id = requestId });
    }

    private async Task PopulateLookupViewBagsAsync(CancellationToken token)
    {
        ViewBag.ItemClassList = (await baseInfoService.GetByParentIdAsync(BaseInfoIds.ItemClassesId, token))
           .ToSelectList(x => x.Id, x => x.Title);
    }

    [HttpGet]
    public async Task<IActionResult> GetItemsByClassAsync(int classId, CancellationToken token)
    {
        var list = (await itemService.GetItemsByClassAsync(classId));

        return Json(list.Select(x => new
        {
            id = x.ItemId,
            text = x.FullName,
        }));
    }

    [HttpGet]
    public async Task<IActionResult> GetItemDetailsByItem(int itemId, CancellationToken token)
    {
        var availability = await requestService.GetItemAvailabilityAsync(
            itemId, CurrentUserRecipientId.GetValueOrDefault(), token);

        if (availability.CurrentCount <= 0)
        {
            TempData["ToastMessage"] = "موجودی این کالا صفر است و امکان ثبت درخواست برای آن وجود ندارد.";
            TempData["ToastType"] = "error";
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
                new() { Header = "شماره درخواست", PropertyName = nameof(RequestListDto.RequestNumber),   Sortable = true  },
                new() { Header = "تاریخ",          PropertyName = nameof(RequestListDto.SendDateShamsi),  Sortable = true },
                new() { Header = "وضعیت",          PropertyName = nameof(RequestListDto.Status),          Sortable = true,
                    IsBadgeColumn = true,
                    BadgeMap = new Dictionary<string, (string Text, string CssClass)>
                    {
                        ["Label"]     = ("در حال ثبت",  "badge-warning"),
                        ["Draft"]     = ("پیش‌نویس",    "badge-secondary"),
                        ["Sent"]      = ("ارسال شده",   "badge-info"),
                        ["Confirmed"] = ("تایید شده",   "badge-success"),
                        ["Rejected"]  = ("رد شده",      "badge-danger"),

                    }},
                new() { Header = "ثبت شده توسط",  PropertyName = nameof(RequestListDto.EnteredByName),   Sortable = true },
                new() { Header = "تاریخ تایید",    PropertyName = nameof(RequestListDto.ConfirmDateShamsi), Sortable = true },
                new() { Header = "عملیات", IsActionColumn = true }
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
                    LinkClass       = "btn-outline-primary",
                }
            ]
        };

        return View(model);
    }

    public async Task<IActionResult> ReceivedRequestLines(int? id, CancellationToken token)
    {
        var dto = await requestService.GetById(id.Value, token);
        if (dto == null) return NotFound();

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
        var existing = await requestService.GetById(id, token);
        if (existing == null) return NotFound();

        // فقط درخواست‌های ارسال‌شده قابل تاییدن (دفاع سمت سرور، مستقل از UI)
        if (existing.RequestStatus != (int)RequestStatusEnum.Sent)
        {
            TempData["ToastMessage"] = "فقط درخواست‌های ارسال‌شده قابل تایید هستند";
            TempData["ToastType"] = "error";
            return RedirectToAction("ReceivedRequestLines", new { id });
        }

        await requestService.ChangeStatus(id, RequestStatusEnum.Confirmed, "", token);

        TempData["ToastMessage"] = "درخواست تایید شد";
        TempData["ToastType"] = "success";
        return RedirectToAction("ReceivedRequests");
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectReceivedRequest(int id, CancellationToken token)
    {
        var existing = await requestService.GetById(id, token);
        if (existing == null) return NotFound();

        if (existing.RequestStatus != (int)RequestStatusEnum.Sent)
        {
            TempData["ToastMessage"] = "فقط درخواست‌های ارسال‌شده قابل رد هستند";
            TempData["ToastType"] = "error";
            return RedirectToAction("ReceivedRequestLines", new { id });
        }

        await requestService.ChangeStatus(id, RequestStatusEnum.Rejected, "", token);

        TempData["ToastMessage"] = "درخواست رد شد";
        TempData["ToastType"] = "success";
        return RedirectToAction("ReceivedRequests");
    }
}