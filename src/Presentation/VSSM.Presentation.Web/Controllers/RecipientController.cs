using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs.BaseEntities.Recipient;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.Base;
using VSSM.Presentation.Web.Models.Recipient;

namespace VSSM.Presentation.Web.Controllers;

public class RecipientController(
    IRecipientService recipientService,
    IBaseInfoService baseInfoService,
    ILevelService levelService) : BaseController
{
    private readonly ILevelService _levelService = levelService;

    #region Index

    public async Task<IActionResult> Index(
        CancellationToken cancellationToken,
        string? search,
        int? levelId,
        int pageNumber = 1,
        int pageSize = 10,
        string sort = "Id",
        string direction = "asc")
    {
        await LoadDropDowns(cancellationToken);

        var list = await recipientService.GetPagedList(
            search, levelId, pageNumber, pageSize, sort, direction, cancellationToken);

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
                new() { Header = "کد", PropertyName = nameof(RecipientGetDto.Code), Sortable = true },
                new() { Header = "نام کامل", PropertyName = nameof(RecipientGetDto.FullName), Sortable = true },
                new() { Header = "سطح", PropertyName = nameof(RecipientGetDto.LevelName), Sortable = false },
                new() { Header = "طریقه حمل و نقل", PropertyName = nameof(RecipientGetDto.TransportModeName), Sortable = false },
                new() { Header = "روش دریافت", PropertyName = nameof(RecipientGetDto.ReceivingModeName), Sortable = false },
                new() { Header = "وضعیت", PropertyName = nameof(RecipientGetDto.IsActive), Sortable = false, IsBadgeColumn = true,
                    BadgeMap = new Dictionary<string, (string Text, string CssClass)>
                    {
                        ["True"]  = ("فعال",    "badge-success"),
                        ["False"] = ("غیرفعال", "badge-danger"),
                    }},
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
                    IconClass = "fa fa-edit",
                    Method = "GET"
                },
                new()
                {
                    Title = "تغییر وضعیت",
                    Controller = "Recipient",
                    Action = "ToggleActive",
                    RouteIdProperty = nameof(RecipientGetDto.RecipientId),
                    IconClass = "fa fa-exchange",
                    Method = "POST",
                }
            }
        };

        return View(model);
    }

    #endregion

    #region Create / Edit

    public async Task<IActionResult> New(int? id, CancellationToken token)
    {
        await LoadDropDowns(token, id);

        var model = new RecipientViewModel
        {
            IsActive = true
        };

        if (id.HasValue && id.Value > 0)
        {
            var dto = await recipientService.GetById(id.Value, token);
            if (dto == null)
            {
                TempData["ToastMessage"] = "گیرنده مورد نظر یافت نشد";
                TempData["ToastType"] = "error";
                return RedirectToAction(nameof(Index));
            }

            model = new RecipientViewModel
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
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RecipientViewModel model, CancellationToken token)
    {
        await LoadDropDowns(token);

        // ===== اعتبارسنجی فیلدهای اجباری تنظیمات عملیاتی =====
        if (!model.TransportModeId.HasValue || model.TransportModeId == 0)
            ModelState.AddModelError("TransportModeId", "طریقه حمل و نقل اجباری است");

        if (!model.ChainLevel.HasValue || model.ChainLevel == 0)
            ModelState.AddModelError("ChainLevel", "سطح در زنجیره اجباری است");

        if (!model.ReceivingModeId.HasValue || model.ReceivingModeId == 0)
            ModelState.AddModelError("ReceivingModeId", "روش دریافت اجباری است");

        if (!model.ClimateId.HasValue || model.ClimateId == 0)
            ModelState.AddModelError("ClimateId", "شرایط اقلیمی اجباری است");

        if (!model.OwnershipId.HasValue || model.OwnershipId == 0)
            ModelState.AddModelError("OwnershipId", "نوع مالکیت اجباری است");

        if (!model.PowerSourceId.HasValue || model.PowerSourceId == 0)
            ModelState.AddModelError("PowerSourceId", "منبع انرژی اجباری است");

        if (!model.TypeOfRoadId.HasValue || model.TypeOfRoadId == 0)
            ModelState.AddModelError("TypeOfRoadId", "نوع راه اجباری است");

        if (!ModelState.IsValid)
        {
            TempData["ToastMessage"] = "تمام فیلدهای اجباری باید پر شوند";
            TempData["ToastType"] = "error";
            return View("New", model);
        }

        try
        {
            // ===== بررسی کد تکراری =====
            var exists = await recipientService.ExistsByCode(model.Code, model.RecipientId, token);
            if (exists)
            {
                ModelState.AddModelError("Code", "کد وارد شده قبلاً ثبت شده است");
                TempData["ToastMessage"] = "کد وارد شده قبلاً ثبت شده است";
                TempData["ToastType"] = "error";
                return View("New", model);
            }

            if (model.RecipientId == null || model.RecipientId == 0)
            {
                // ---------- CREATE ----------
                var dto = new RecipientCreateDto
                {
                    Code = model.Code?.Trim(),
                    FullName = model.FullName?.Trim(),
                    IsActive = true,
                    TransportModeId = model.TransportModeId,
                    Distance = model.Distance,
                    Population = model.Population,
                    Year = model.Year,
                    Note = model.Note?.Trim(),
                    ParentId = model.ParentType.HasValue ? (int)model.ParentType.Value : (int?)null,
                    StartUp = DateTime.Now,
                    ChainLevel = model.ChainLevel,
                    ReceivingModeId = model.ReceivingModeId,
                    Address1 = model.Address1?.Trim(),
                    Address2 = model.Address2?.Trim(),
                    PostalCode = model.PostalCode?.Trim(),
                    City = model.City?.Trim(),
                    Province = model.Province?.Trim(),
                    Phone = model.Phone?.Trim(),
                    Email = model.Email?.Trim(),
                    LeadTime = model.LeadTime,
                    ClimateId = model.ClimateId,
                    OwnershipId = model.OwnershipId,
                    PowerSourceId = model.PowerSourceId,
                    TypeOfRoadId = model.TypeOfRoadId,
                    HasLandLine = model.HasLandLine,
                    HasInternet = model.HasInternet,
                    DistanceTime = model.DistanceTime
                };

                await recipientService.Create(dto, token);
            }
            else
            {
                // ---------- UPDATE ----------
                var dto = new RecipientEditDto
                {
                    RecipientId = model.RecipientId.Value,
                    Code = model.Code?.Trim(),
                    FullName = model.FullName?.Trim(),
                    IsActive = model.IsActive,
                    TransportModeId = model.TransportModeId,
                    Distance = model.Distance,
                    Population = model.Population,
                    Year = model.Year,
                    Note = model.Note?.Trim(),
                    ParentId = model.ParentType.HasValue ? (int)model.ParentType.Value : (int?)null,
                    StartUp = DateTime.Now,
                    ChainLevel = model.ChainLevel,
                    ReceivingModeId = model.ReceivingModeId,
                    Address1 = model.Address1?.Trim(),
                    Address2 = model.Address2?.Trim(),
                    PostalCode = model.PostalCode?.Trim(),
                    City = model.City?.Trim(),
                    Province = model.Province?.Trim(),
                    Phone = model.Phone?.Trim(),
                    Email = model.Email?.Trim(),
                    LeadTime = model.LeadTime,
                    ClimateId = model.ClimateId,
                    OwnershipId = model.OwnershipId,
                    PowerSourceId = model.PowerSourceId,
                    TypeOfRoadId = model.TypeOfRoadId,
                    HasLandLine = model.HasLandLine,
                    HasInternet = model.HasInternet,
                    DistanceTime = model.DistanceTime
                };

                await recipientService.Update(dto, token);
            }

            TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
            TempData["ToastType"] = "success";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException?.Message.Contains("IX_TblRecipients_Code") == true ||
                ex.InnerException?.Message.Contains("Unique") == true ||
                ex.InnerException?.Message.Contains("duplicate") == true)
            {
                TempData["ToastMessage"] = "کد وارد شده قبلاً ثبت شده است";
                TempData["ToastType"] = "error";
            }
            else
            {
                TempData["ToastMessage"] = "خطا در ارتباط با دیتابیس: " + (ex.InnerException?.Message ?? ex.Message);
                TempData["ToastType"] = "error";
            }
            return View("New", model);
        }
        catch (Exception ex)
        {
            TempData["ToastMessage"] = "خطا در انجام عملیات: " + ex.Message;
            TempData["ToastType"] = "error";
            return View("New", model);
        }
    }

    #endregion

    #region Toggle Active

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id, CancellationToken token)
    {
        try
        {
            var result = await recipientService.ToggleActive(id, token);
            TempData["ToastMessage"] = result
                ? "وضعیت گیرنده با موفقیت به فعال تغییر یافت"
                : "وضعیت گیرنده با موفقیت به غیرفعال تغییر یافت";
            TempData["ToastType"] = "success";
        }
        catch (Exception ex)
        {
            TempData["ToastMessage"] = "خطا در تغییر وضعیت: " + ex.Message;
            TempData["ToastType"] = "error";
        }

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Private Methods

    private async Task LoadDropDowns(CancellationToken token, int? id = null)
    {
        var currentUserLevel = CurrentUserLevel ?? 0;

        var allLevels = await baseInfoService.GetByParentIdAsync(91, token);

        var filteredLevels = allLevels
         .Where(x => int.TryParse(x.Code, out int codeValue) && codeValue <= currentUserLevel)
         .Select(x => new SelectListItem
         {
             Value = x.Id.ToString(),
             Text = x.Title
         })
         .ToList();
        ViewBag.LevelList = filteredLevels;

        ViewBag.LevelListForFilter = filteredLevels
            .Prepend(new SelectListItem { Text = "همه", Value = "" })
            .ToList();

        ViewBag.TransportList = (await baseInfoService.GetByParentIdAsync(68, token))
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Title
            }).ToList();

        ViewBag.ReceivingList = (await baseInfoService.GetByParentIdAsync(64, token))
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Title
            }).ToList();

        ViewBag.ClimateList = (await baseInfoService.GetByParentIdAsync(78, token))
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Title
            }).ToList();

        ViewBag.OwnershipList = (await baseInfoService.GetByParentIdAsync(81, token))
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Title
            }).ToList();

        ViewBag.PowerSourceList = (await baseInfoService.GetByParentIdAsync(75, token))
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Title
            }).ToList();

        ViewBag.RoadTypeList = (await baseInfoService.GetByParentIdAsync(71, token))
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Title
            }).ToList();

        var parents = (await recipientService.GetActiveRecipients(token))
            .Select(e => new SelectListItem
            {
                Value = e.RecipientId.ToString(),
                Text = e.FullName
            })
            .ToList();

        if (id is not null)
            parents = parents.Where(t => t.Value != id.ToString()).ToList();

        ViewBag.ParentTypeList = parents;
    }

    #endregion
}