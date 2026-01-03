using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuGet.Common;
using VSSM.Application.DTOs.BaseEntities.Arrival;
using VSSM.Application.DTOs.RequestEntities;
using VSSM.Application.Helpers;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Application.Interfaces.RequestEntities;
using VSSM.Application.Services.BaseEntities;
using VSSM.Application.Services.RequestEntities;
using VSSM.Presentation.Web.Models.Arrival;
using VSSM.Presentation.Web.Models.Request;

namespace VSSM.Presentation.Web.Controllers
{
    [Authorize]
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
                                                                    cancellationToken);

            
            return View(listModel);

        }
        public async Task<IActionResult> New(CancellationToken token)
        {
            await LoadDropDowns(token);
            return View();
        }

        private async Task<IActionResult> AddLine(RequestViewModel model, CancellationToken token)
        {
            if (model.Id == 0)
            {
                var createDto = new RequestCreateDto
                {
                    Status = 0,
                    SenderId = model.Id,
                    ConfirmDate = DateOnly.FromDateTime(DateTime.Now),
                    EnteredBy = model.EnteredBy,
                    FiscalPeriodId = 0,
                    Note = "",
                    RequestNumber = "1",
                    ReceiverId = 1,
                    SendDate = DateOnly.FromDateTime(DateTime.Now),

                    Lines = new List<RequestLineDto>()
                };

                var newId = await _requestService.Create(createDto, token);
                model.Id = newId;
            }


            var lineDto = new RequestLineDto
            {
                
            };


            TempData["ToastMessage"] = "کالا اضافه شد";
            TempData["ToastType"] = "success";

            return RedirectToAction("New", new { id = model.Id });
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
            return null;
        }

    }
}
