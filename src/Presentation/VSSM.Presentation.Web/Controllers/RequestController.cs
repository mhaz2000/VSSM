using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuGet.Common;
using System.Collections.Generic;
using VSSM.Application.DTOs.BaseEntities.Arrival;
using VSSM.Application.DTOs.RequestEntities;
using VSSM.Application.Helpers;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Application.Interfaces.RequestEntities;
using VSSM.Application.Services.BaseEntities;
using VSSM.Application.Services.RequestEntities;
using VSSM.Domain.Enums;
using VSSM.Presentation.Web.Models.Arrival;
using VSSM.Presentation.Web.Models.Request;

namespace VSSM.Presentation.Web.Controllers
{
    
    public class RequestController : BaseController
    {
        private readonly IItemClassService _itemClassService;
        private readonly IItemService _itemService;
        private readonly IRequestService _requestService;
        public RequestController(IRequestService requestService, IItemClassService itemClassService, IItemService itemService)
        {
            _requestService = requestService;
            _itemClassService = itemClassService;
            _itemService = itemService;
        }
        public async Task<IActionResult> Index(CancellationToken cancellationToken, string? search,
                                                int page = 1,
                                                int pageSize = 10,
                                                string sortField = "Orders",
                                                string sortOrder = "desc")
        {
            var listModel = await _requestService.GetPagedList(search,
                                                                    page,
                                                                    pageSize,
                                                                    sortField,
                                                                    sortOrder,
                                                                    CurrentUserRecipientId.GetValueOrDefault(),
                                                                    cancellationToken);

            
            return View(listModel);

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
            var dto = await _requestService.GetById(id.Value, token);
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
                if(command == "StatusSent")
                {
                    await _requestService.ChangeStatus(model.Id.Value, RequestStatusEnum.Sent, "", token);
                    TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
                    TempData["ToastType"] = "success";
                    return RedirectToAction("Index");
                }
                if (command == "StatusDraft")
                {
                    await _requestService.ChangeStatus(model.Id.Value, RequestStatusEnum.Draft, "", token);
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

                var existing = await _requestService.GetById(model.Id.Value, token);
                if(existing == null) return RedirectToAction("New");
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

                if(model.NewLine.ItemClassId == null || model.NewLine.ItemClassId == 0)
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

                var newId = await _requestService.Create(createDto, token);
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

            await _requestService.AddLine(model.Id.Value, lineDto, token);

            TempData["ToastMessage"] = "کالا اضافه شد";
            TempData["ToastType"] = "success";

            return RedirectToAction("New", new { id = model.Id });
        }

        public async Task<IActionResult> DeleteRequest(int id, CancellationToken token)
        {
            if (id != null && id > 0 )
            {
                var existing = await _requestService.GetById(id, token);
                if (existing.RequestStatus == (int)RequestStatusEnum.Label)
                {
                    await _requestService.Delete(id);
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
            await _requestService.DeleteLine(id, token);

            TempData["ToastMessage"] = "سطر حذف شد";
            TempData["ToastType"] = "success";

            return RedirectToAction("New", new { id = requestId });
        }

        private async Task LoadDropDowns(CancellationToken token)
        {
            ViewBag.ItemList = (await _itemService.GetActives(token))
                .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.FullName })
                .ToList();

            ViewBag.ItemClassList = (await _itemClassService.GetActives(token))
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
            var list = (await _itemService.GetActives(token)).Where(z => z.ItemClassId == classId);

            return Json(list.Select(x => new
            {
                id = x.Id,
                text = x.FullName,
            }));
        }

        [HttpGet]
        public async Task<IActionResult> GetItemDetailsByItem(int itemId, CancellationToken token)
        {
            return Json(new
            {
                anncount = 0,
                arivcount = 0,
                confirmcount = 0,
                currentcount = 0,
                offercount = 0,
                usedcount = 0
            });
        }




        public async Task<IActionResult> ReceivedRequests(CancellationToken cancellationToken, string? search,
                                                int page = 1,
                                                int pageSize = 10,
                                                string sortField = "Orders",
                                                string sortOrder = "desc")
        {
            var listModel = await _requestService.GetReceivedRequestsPagedList(search,
                                                                    page,
                                                                    pageSize,
                                                                    sortField,
                                                                    sortOrder,
                                                                    CurrentUserRecipientId.GetValueOrDefault(),
                                                                    cancellationToken);


            return View(listModel);

        }


        public async Task<IActionResult> ReceivedRequestLines(int? id, CancellationToken token)
        {
            var dto = await _requestService.GetById(id.Value, token);
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
}
