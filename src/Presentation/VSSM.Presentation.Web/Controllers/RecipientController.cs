using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VSSM.Application.DTOs.BaseEntities.Recipient;
using VSSM.Application.Helpers;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Domain.Enums;
using VSSM.Presentation.Web.Models.Recipient;

namespace VSSM.Presentation.Web.Controllers
{
    public class RecipientController : Controller
    {
        private readonly IRecipientService _recipientService;
        private readonly ITransportModeService _transportService;
        private readonly IReceivingModeService _receivingService;
        private readonly IClimateService _climateService;
        private readonly IOwnershipService _ownershipService;
        private readonly IPowerSourceService _powerSourceService;
        private readonly IRoadTypeService _roadTypeService;

        public RecipientController(
            IRecipientService recipientService,
            ITransportModeService transportService,
            IReceivingModeService receivingService,
            IClimateService climateService,
            IOwnershipService ownershipService,
            IPowerSourceService powerSourceService,
            IRoadTypeService roadTypeService)
        {
            _recipientService = recipientService;
            _transportService = transportService;
            _receivingService = receivingService;
            _climateService = climateService;
            _ownershipService = ownershipService;
            _powerSourceService = powerSourceService;
            _roadTypeService = roadTypeService;
        }

        // LIST
        public async Task<IActionResult> Index(
            string? search,
            int page = 1,
            int pageSize = 10,
            string sortField = "Code",
            string sortOrder = "asc",
            CancellationToken token = default)
        {
            var list = await _recipientService.GetPagedList(
                search, page, pageSize, sortField, sortOrder, token);

            return View(list);
        }

        // NEW / EDIT
        public async Task<IActionResult> New(int? id, CancellationToken token)
        {
            await LoadDropDowns(token);

            if (id == null)
                return View(new RecipientViewModel());

            var dto = await _recipientService.GetById(id.Value, token);

            var model = new RecipientViewModel
            {
                RecipientId = dto.RecipientId,
                Code = dto.Code,
                FullName = dto.FullName,
                IsActive = dto.IsActive,
                TransportModeId = dto.TransportModeId,
                Distance = dto.Distance,
                Population = dto.Population,
                Year = dto.Year,
                Note = dto.Note,
                ChainLevel = dto.ChainLevel,
                ReceivingModeId = dto.ReceivingModeId,
                ParentType = dto.ParentId.HasValue
                    ? (RecipientTypeEnum?)dto.ParentId.Value
                    : null,
                StartUp = PersianDateHelper.ToStringDate(dto.StartUp),
                Address1 = dto.Address1,
                Address2 = dto.Address2,
                PostalCode = dto.PostalCode,
                City = dto.City,
                Province = dto.Province,
                Phone = dto.Phone,
                Email = dto.Email,
                LeadTime = dto.LeadTime,
                ClimateId = dto.ClimateId,
                OwnershipId = dto.OwnershipId,
                PowerSourceId = dto.PowerSourceId,
                TypeOfRoadId = dto.TypeOfRoadId,
                HasLandLine = dto.HasLandLine,
                HasInternet = dto.HasInternet,
                DistanceTime = dto.DistanceTime
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(RecipientViewModel model, CancellationToken token)
        {
            await LoadDropDowns(token);

            if (!ModelState.IsValid)
                return View("New", model);

            if (model.RecipientId == null || model.RecipientId == 0)
            {
                // CREATE
                var dto = new RecipientCreateDto
                {
                    Code = model.Code,
                    FullName = model.FullName,
                    IsActive = model.IsActive,
                    TransportModeId = model.TransportModeId,
                    Distance = model.Distance,
                    Population = model.Population,
                    Year = model.Year,
                    Note = model.Note,
                    ParentId = model.ParentType.HasValue ? (int)model.ParentType.Value : (int?)null,
                    StartUp = PersianDateHelper.ToDateTime(model.StartUp),
                    ChainLevel = model.ChainLevel,
                    ReceivingModeId = model.ReceivingModeId,
                    Address1 = model.Address1,
                    Address2 = model.Address2,
                    PostalCode = model.PostalCode,
                    City = model.City,
                    Province = model.Province,
                    Phone = model.Phone,
                    Email = model.Email,
                    LeadTime = model.LeadTime,
                    ClimateId = model.ClimateId,
                    OwnershipId = model.OwnershipId,
                    PowerSourceId = model.PowerSourceId,
                    TypeOfRoadId = model.TypeOfRoadId,
                    HasLandLine = model.HasLandLine,
                    HasInternet = model.HasInternet,
                    DistanceTime = model.DistanceTime
                };

                await _recipientService.Create(dto, token);
            }
            else
            {
                // UPDATE
                var dto = new RecipientEditDto
                {
                    RecipientId = model.RecipientId.Value,
                    Code = model.Code,
                    FullName = model.FullName,
                    IsActive = model.IsActive,
                    TransportModeId = model.TransportModeId,
                    Distance = model.Distance,
                    Population = model.Population,
                    Year = model.Year,
                    Note = model.Note,
                    ParentId = model.ParentType.HasValue ? (int)model.ParentType.Value : (int?)null,
                    StartUp = PersianDateHelper.ToDateTime(model.StartUp),
                    ChainLevel = model.ChainLevel,
                    ReceivingModeId = model.ReceivingModeId,
                    Address1 = model.Address1,
                    Address2 = model.Address2,
                    PostalCode = model.PostalCode,
                    City = model.City,
                    Province = model.Province,
                    Phone = model.Phone,
                    Email = model.Email,
                    LeadTime = model.LeadTime,
                    ClimateId = model.ClimateId,
                    OwnershipId = model.OwnershipId,
                    PowerSourceId = model.PowerSourceId,
                    TypeOfRoadId = model.TypeOfRoadId,
                    HasLandLine = model.HasLandLine,
                    HasInternet = model.HasInternet,
                    DistanceTime = model.DistanceTime
                };

                await _recipientService.Update(dto, token);
            }

            TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
            TempData["ToastType"] = "success";

            return RedirectToAction("Index");
        }

        private async Task LoadDropDowns(CancellationToken token)
        {
            ViewBag.TransportList = (await _transportService.GetAll(token))
                .Select(x => new SelectListItem
                {
                    Value = x.TransportModeId.ToString(),
                    Text = x.FullName
                }).ToList();

            ViewBag.ReceivingList = (await _receivingService.GetAllAsync(token))
                .Select(x => new SelectListItem
                {
                    Value = x.ReceivingModeId.ToString(),
                    Text = x.FullName
                }).ToList();

            ViewBag.ClimateList = (await _climateService.GetAllAsync(token))
                .Select(x => new SelectListItem
                {
                    Value = x.ClimateId.ToString(),
                    Text = x.FullName
                }).ToList();

            ViewBag.OwnershipList = (await _ownershipService.GetAll(token))
                .Select(x => new SelectListItem
                {
                    Value = x.OwnershipId.ToString(),
                    Text = x.FullName
                }).ToList();

            ViewBag.PowerSourceList = (await _powerSourceService.GetAll(token))
                .Select(x => new SelectListItem
                {
                    Value = x.PowerSourceId.ToString(),
                    Text = x.FullName
                }).ToList();

            ViewBag.RoadTypeList = (await _roadTypeService.GetAll(token))
                .Select(x => new SelectListItem
                {
                    Value = x.RoadTypeId.ToString(),
                    Text = x.FullName
                }).ToList();

            ViewBag.ParentTypeList = Enum.GetValues(typeof(RecipientTypeEnum))
                .Cast<RecipientTypeEnum>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = e.GetDisplayName()
                })
                .ToList();
        }
    }
}
