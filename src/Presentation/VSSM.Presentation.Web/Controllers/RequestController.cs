using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VSSM.Application.DTOs.RequestEntities;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Application.Interfaces.RequestEntities;
using VSSM.Application.Services.RequestEntities;
using VSSM.Domain.Enums;
using VSSM.Presentation.Web.Models.Base;
using VSSM.Presentation.Web.Models.Request;

namespace VSSM.Presentation.Web.Controllers;


public class RequestController(
    IRequestService requestService,
    IItemClassService itemClassService,
    IItemService itemService) : BaseController
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
                Action          = "New",
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
                    return (val != "0").ToString().ToLower(); // "true" = skip
                }
            }
            ]
        };

        return View(model);
    }
    public async Task<IActionResult> New(int? id, CancellationToken token)
    {
        // Create mode
        if (id == null || id == 0)
        {
            //if (CurrentUserLevel == 1)
            //{
            //    TempData["ToastMessage"] = "عدم امکان ثبت درخواست برای بالاترین سطح";
            //    TempData["ToastType"] = "warning";
            //    return RedirectToAction("Index");
            //}

            var vm = new RequestViewModel
            {
                RequestLines = new List<RequestLinesViewModel>(),
                NewLine = new RequestLinesViewModel()
            };
            await LoadDropDowns(token);
            return View(vm);
        }

        // Edit mode
        var dto = await requestService.GetById(id.Value, token);
        if (dto == null) return NotFound();
        await LoadDropDowns(token);
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
                Anncount = r.Anncount,
                Arivcount = r.Arivcount,
                Confirmcount = r.Confirmcount,
                Currentcount = r.Currentcount,
                DosePerVialId = r.DosePerVialId,
                Forperiod = r.Forperiod,
                ItemClassId = r.ItemClassId,
                ItemId = r.ItemId,
                ItemName = r.ItemName,
                Note = r.Note,
                Offercount = r.Offercount,
                Reqcount = r.Reqcount,
                Usedcount = r.Usedcount
            }).ToList(),

            NewLine = new RequestLinesViewModel()
        };

        return View(model);

    }

    public async Task<IActionResult> Create(RequestViewModel model, string command, CancellationToken token)
    {
        await LoadDropDowns(token);

        if (model.Id.HasValue)
        {
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

            var existing = await requestService.GetById(model.Id.Value, token);
            if (existing == null) return RedirectToAction("New");
            if (existing.RequestStatus == (int)RequestStatusEnum.Confirmed)
            {
                TempData["ToastMessage"] = "این ورود تایید شده است و امکان ویرایش ندارد";
                TempData["ToastType"] = "error";
                return RedirectToAction("Edit", new { id = model.Id.Value });
            }
        }

        return await AddLine(model, token);
    }

    private async Task<IActionResult> AddLine(RequestViewModel model, CancellationToken token)
    {
        if (!model.Id.HasValue || model.Id == 0)
        {
            var createDto = new RequestCreateDto
            {
                Status = RequestStatusEnum.Label,
                SenderId = CurrentUserRecipientId,
                ConfirmDate = DateOnly.FromDateTime(DateTime.Now),
                EnteredBy = CurrentUserId,
                FiscalPeriodId = null,
                Note = "",
                RequestNumber = "",
                ReceiverId = CurrentUserRecipientParentId,
                SendDate = DateOnly.FromDateTime(DateTime.Now),

                Lines = new List<RequestLineDto>()
            };

            if (model.NewLine.ItemClassId == null || model.NewLine.ItemClassId == 0)
            {
                TempData["ToastMessage"] = "انتخاب نوع کالا اجباری است";
                TempData["ToastType"] = "error";
                return RedirectToAction("New", new { id = model.Id.Value });
            }
            if (model.NewLine.ItemId == null || model.NewLine.ItemId == 0)
            {
                TempData["ToastMessage"] = "انتخاب کالا اجباری است";
                TempData["ToastType"] = "error";
                return RedirectToAction("New", new { id = model.Id.Value });
            }

            var newId = await requestService.Create(createDto, token);
            model.Id = newId;
        }

        var lineDto = new RequestLineDto
        {
            Anncount = model.NewLine.Anncount,
            Arivcount = model.NewLine.Arivcount,
            Reqcount = model.NewLine.Reqcount,
            Confirmcount = model.NewLine.Confirmcount,
            Currentcount = model.NewLine.Currentcount,
            Offercount = model.NewLine.Offercount,
            Usedcount = model.NewLine.Usedcount,
            Forperiod = model.NewLine.Forperiod,
            ItemClassId = model.NewLine.ItemClassId,
            ItemId = model.NewLine.ItemId,
            DosePerVialId = model.NewLine.Forperiod,
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
        if (id != null && id > 0)
        {
            var existing = await requestService.GetById(id, token);
            if (existing.RequestStatus == (int)RequestStatusEnum.Label)
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

    private async Task LoadDropDowns(CancellationToken token)
    {
        ViewBag.Items = (await itemService.GetActives(token))
            .Select(x => new SelectListItem { Value = x.ItemId.ToString(), Text = x.FullName })
            .ToList();

        ViewBag.ItemClassList = (await itemClassService.GetActives(token))
         .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name })
         .ToList();



        ViewBag.ForPeriodList = new List<SelectListItem> {
            new SelectListItem { Value = "1", Text = "1" },
            new SelectListItem { Value = "2", Text = "2" },
            new SelectListItem { Value = "3", Text = "3" },
            new SelectListItem { Value = "4", Text = "4" },
            new SelectListItem { Value = "5", Text = "5" },
            new SelectListItem { Value = "6", Text = "6" }
        };

    }

    [HttpGet]
    public async Task<IActionResult> GetItemsByClass(int classId, CancellationToken token)
    {
        var list = (await itemService.GetActives(token)).Where(z => z.FkClassId == classId);

        return Json(list.Select(x => new
        {
            id = x.ItemId,
            text = x.FullName,
        }));
    }

    [HttpGet]
    public async Task<IActionResult> GetItemDetailsByItem(int itemId, CancellationToken token)
    {
        return Json(new
        {
            anncount = 0, //مقدار مجاز
            arivcount = 0, // مقدار ورودی
            confirmcount = 0, //
            currentcount = 0, //تعداد موجودی انبار
            offercount = 0, //تعداد پیشنهادی
            usedcount = 0 //تعداد مصرف شده
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
            search, page, pageSize, sort, direction,
            CurrentUserRecipientId.GetValueOrDefault(), cancellationToken);

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
                Anncount = r.Anncount,
                Arivcount = r.Arivcount,
                Confirmcount = r.Confirmcount,
                Currentcount = r.Currentcount,
                DosePerVialId = r.DosePerVialId,
                Forperiod = r.Forperiod,
                ItemClassId = r.ItemClassId,
                ItemId = r.ItemId,
                ItemName = r.ItemName,
                Note = r.Note,
                Offercount = r.Offercount,
                Reqcount = r.Reqcount,
                Usedcount = r.Usedcount
            }).ToList(),

            NewLine = new RequestLinesViewModel()
        };

        return View(model);

    }

}
