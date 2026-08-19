
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VSSM.Application.DTOs.BaseEntities.Store;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.Base;
using VSSM.Presentation.Web.Models.Store;

namespace VSSM.Presentation.Web.Controllers
{

    public class StoreController : BaseController
    {
        private readonly IStoreService _storeService;
        private readonly IBaseInfoService _baseInfoService;
        private readonly IRecipientService _recipientService;
        private readonly ISourceService _sourceService;

        public StoreController(
            IStoreService storeService,
            IRecipientService recipientService,
            IBaseInfoService baseInfoService,
            ISourceService sourceService)
        {
            _storeService = storeService;
            _recipientService = recipientService;
            _sourceService = sourceService;
            _baseInfoService = baseInfoService;
        }

        public async Task<IActionResult> Index(
         string? search,
         int? typeId,
         int page = 1,
         int pageSize = 10,
         CancellationToken token = default)
        {
            var storeTypes = await _baseInfoService.GetByParentIdAsync(56, token);

            ViewBag.StoreTypeList = storeTypes is null ? [] : storeTypes
                .Select(x => new SelectListItem
                {
                    Text = x.Title,
                    Value = x.Id.ToString()
                })
                .ToList();

            var list = await _storeService.GetPagedList(search, typeId, page, pageSize, token);

            var model = new GridViewModel
            {
                SearchTerm = search,
                Data = new PagedResult<object>
                {
                    Items = list.Items.Cast<object>().ToList(),
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalCount = list.Paging.TotalPages
                },
                Columns = new List<GridColumn>
            {
                new() { Header = "نام", PropertyName = nameof(StoreListDto.FullName),  Sortable = false },
                new() { Header = "نوع انبار", PropertyName = nameof(StoreListDto.TypeName), Sortable = false },
                new() { Header = "حجم کلی فضا (لیتر)", PropertyName = nameof(StoreListDto.Volume), Sortable = false },
                new() { Header = "حجم مفید (لیتر)", PropertyName = nameof(StoreListDto.FreeVolume),  Sortable = false },
                new() { Header = "وضعیت", PropertyName = nameof(StoreListDto.Code),  Sortable = false },
                new() { Header = "عملیات", IsActionColumn = true}
            },
                Actions = new List<GridAction>
            {
                new()
                {
                    Title = "ویرایش",
                    Controller = "Store",
                    Action = "New",
                    RouteIdProperty = nameof(StoreListDto.StoreId),
                    LinkClass = "",
                    IconClass = "fa fa-edit",
                }
            }
            };


            return View(model);
        }

        public async Task<IActionResult> New(int? id, CancellationToken cancellationToken)
        {
            await LoadDropDowns(cancellationToken);

            StoreViewModel models = new StoreViewModel();

            if (id.HasValue && id.Value > 0)
            {
                var dto = await _storeService.GetById(id.Value, cancellationToken);

                models = new StoreViewModel
                {
                    StoreId = dto.Id,
                    Code = dto.Code,
                    Name = dto.Name,
                    TypeId = dto.TypeId,
                    FreeVolume = dto.FreeVolume,
                    Volume = dto.Volume,
                    Note = dto.Note,
                    IsActive = dto.IsActive,
                    RecipientId = dto.RecipientId,

                    Year = dto.Year,
                };
            }

            return View(models);
        }

        [HttpPost]
        public async Task<IActionResult> Create(StoreViewModel storeView, CancellationToken cancellationToken)
        {
            await LoadDropDowns(cancellationToken);
            if (!ModelState.IsValid)
            {

                return View("New", storeView);
            }

            if (storeView.StoreId == null || storeView.StoreId == 0)
            {
                var dto = new StoreCreateDto
                {
                    Code = storeView.Code,
                    Name = storeView.Name,
                    TypeId = storeView.TypeId.Value,
                    FreeVolume = storeView.FreeVolume.Value,
                    Volume = storeView.Volume,
                    Note = storeView.Note,
                    IsActive = storeView.IsActive,
                    RecipientId = storeView.RecipientId,

                    Year = storeView.Year,

                };

                await _storeService.Create(dto, cancellationToken);
            }
            else
            {
                var dto = new StoreEditDto
                {
                    Id = storeView.StoreId.Value,
                    Code = storeView.Code,
                    Name = storeView.Name,
                    TypeId = storeView.TypeId.Value,
                    FreeVolume = storeView.FreeVolume.Value,
                    Volume = storeView.Volume,
                    Note = storeView.Note,
                    IsActive = storeView.IsActive,
                    RecipientId = storeView.RecipientId,

                    Year = storeView.Year,
                };

                await _storeService.Update(dto, cancellationToken);
            }

            TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
            TempData["ToastType"] = "success";

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            return await New(id, cancellationToken);
        }

        private async Task LoadDropDowns(CancellationToken cancellationToken)
        {
            var storeTypes = await _baseInfoService.GetByParentIdAsync(56, cancellationToken);
            var recipients = await _recipientService.GetActiveRecipients(cancellationToken);
            var powerSources = await _baseInfoService.GetByParentIdAsync(75 ,cancellationToken);
            var sources = await _sourceService.GetAll(cancellationToken);

            ViewBag.StoreTypeList = storeTypes is null ? [] : storeTypes
                .Select(x => new SelectListItem
                {
                    Text = x.Title,
                    Value = x.Id.ToString()
                })
                .ToList();

            ViewBag.RecipientList = recipients
                .Select(x => new SelectListItem
                {
                    Text = x.FullName,
                    Value = x.RecipientId.ToString()
                })
                .ToList();

            ViewBag.PowerSourceList = powerSources
                .Select(x => new SelectListItem
                {
                    Text = x.Title,
                    Value = x.Id.ToString()
                })
                .ToList();

            ViewBag.SourceList = sources
                .Select(x => new SelectListItem
                {
                    Text = x.FullName,
                    Value = x.SourceId.ToString()
                })
                .ToList();
        }
    }
}
