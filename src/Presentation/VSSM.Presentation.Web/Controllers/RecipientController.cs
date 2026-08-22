using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VSSM.Application.DTOs.BaseEntities.Recipient;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.Base;
using VSSM.Presentation.Web.Models.Recipient;

namespace VSSM.Presentation.Web.Controllers
{
    
    public class RecipientController : BaseController
    {
        private readonly IRecipientService _recipientService;
        private readonly ILevelService _levelService;
        private readonly IBaseInfoService _baseInfoService;

        public RecipientController(
            IRecipientService recipientService,
            IBaseInfoService baseInfoService,
            ILevelService levelService)
        {
            _recipientService = recipientService;
            _levelService = levelService;
            _baseInfoService = baseInfoService;
        }

        // LIST
        public async Task<IActionResult> Index(CancellationToken cancellationToken,
            string? search,
            int pageNumber = 1,
            int pageSize = 10,
            string sort = "FullName",
            string direction = "asc")
        {
            var list = await _recipientService.GetPagedList(
                search, pageNumber, pageSize, sort, direction, cancellationToken);

            var model = new GridViewModel
            {
                SearchTerm = search,
                CurrentSort = sort,
                CurrentDirection = direction,
                Data = new PagedResult<object>
                {
                    Items = list.Items.Cast<object>().ToList(),
                    PageNumber = list.Paging.Page,
                    PageSize = list.Paging.PageSize,
                    TotalCount = list.Paging.TotalItems
                },
                Columns = new List<GridColumn>
                {
                    new() { Header = "نام کامل", PropertyName = nameof(RecipientGetDto.FullName), Sortable = true },
                    new() { Header = "کد", PropertyName = nameof(RecipientGetDto.Code), Sortable = true },
                    new() { Header = "فعال", PropertyName = nameof(RecipientGetDto.IsActiveText), Sortable = false },
                    new() { Header = "طریقه حمل و نقل", PropertyName = nameof(RecipientGetDto.TransportModeName), Sortable = false },
                    new() { Header = "روش دریافت", PropertyName = nameof(RecipientGetDto.ReceivingModeName), Sortable = false },
                    new() { Header = "عملیات", IsActionColumn = true }
                },
                Actions = new List<GridAction>
                {
                    new()
                    {
                        Title = "ویرایش",
                        Controller = "Recipient",
                        Action = "New",
                        RouteIdProperty = nameof(RecipientGetDto.RecipientId),
                        IconClass = "fa fa-edit"
                    }
                }
            };

            return View(model);
        }

        // NEW / EDIT
        public async Task<IActionResult> New(int? id, CancellationToken token)
        {
            await LoadDropDowns(token, id);

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
                ParentType = dto.ParentId,

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
        [ValidateAntiForgeryToken]
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
                    StartUp = DateTime.Now,
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
                    StartUp = DateTime.Now,
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

        private async Task LoadDropDowns(CancellationToken token, int? id = null)
        {
            ViewBag.TransportList = (await _baseInfoService.GetByParentIdAsync(68,token))
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Title
                }).ToList();


            ViewBag.ReceivingList = (await _baseInfoService.GetByParentIdAsync(64, token))
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Title
                }).ToList();

            ViewBag.ClimateList = (await _baseInfoService.GetByParentIdAsync(78,token))
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Title
                }).ToList();

            ViewBag.OwnershipList = (await _baseInfoService.GetByParentIdAsync(81,token))
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Title
                }).ToList();

            ViewBag.PowerSourceList = (await _baseInfoService.GetByParentIdAsync(75,token))
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Title
                }).ToList();

            ViewBag.RoadTypeList = (await _baseInfoService.GetByParentIdAsync(71, token))
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Title
                }).ToList();

            var parents = (await _recipientService.GetActiveRecipients(token))
                .Select(e => new SelectListItem
                {
                    Value = e.RecipientId.ToString(),
                    Text = e.FullName
                })
                .ToList();

            if (id is not null)
                parents = parents.Where(t => t.Value != id.ToString()).ToList();

            ViewBag.ParentTypeList = parents;



            ViewBag.LevelList = (await _levelService.GetAll(token))
             .Select(e => new SelectListItem
             {
                 Value = e.LevelId.ToString(),
                 Text = e.ShipCount.ToString()
             })
             .ToList();
        }

        public override bool Equals(object? obj)
        {
            return obj is RecipientController controller &&
                   EqualityComparer<ILevelService>.Default.Equals(_levelService, controller._levelService);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_levelService);
        }
    }
}
