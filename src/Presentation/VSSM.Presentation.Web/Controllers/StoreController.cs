using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VSSM.Application.DTOs.BaseEntities.Store;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.Base;
using VSSM.Presentation.Web.Models.Store;

namespace VSSM.Presentation.Web.Controllers;

public class StoreController(
    IStoreService storeService,
    IRecipientService recipientService,
    IBaseInfoService baseInfoService) : BaseController
{
    #region Index

    public async Task<IActionResult> Index(
        string? search,
        string? recipientId,
        int? typeId,
        int page = 1,
        int pageSize = 10,
        CancellationToken token = default)
    {
        await LoadDropDowns(token);

        var list = await storeService.GetPagedList(search, recipientId, typeId, page, pageSize, token);

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
                new() { Header = "کد", PropertyName = nameof(StoreListDto.Code), Sortable = true },
                new() { Header = "نام انبار", PropertyName = nameof(StoreListDto.FullName), Sortable = true },
                new() { Header = "نوع انبار", PropertyName = nameof(StoreListDto.TypeName), Sortable = false },
                new() { Header = "مالک", PropertyName = nameof(StoreListDto.RecipientName), Sortable = false },
                new() { Header = "وضعیت", PropertyName = nameof(StoreListDto.IsActive), Sortable = false, IsBadgeColumn = true,
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
                    Controller = "Store",
                    Action = "New",
                    RouteIdProperty = nameof(StoreListDto.StoreId),
                    IconClass = "fa fa-edit",
                },
                new()
                {
                    Title = "تغییر وضعیت",
                    Controller = "Store",
                    Action = "ToggleActive",
                    RouteIdProperty = nameof(StoreListDto.StoreId),
                    IconClass = "fa fa-exchange",
                    Method = "POST"
                }
            }
        };

        return View(model);
    }

    #endregion

    #region Create / Edit

    public async Task<IActionResult> New(int? id, CancellationToken cancellationToken)
    {
        await LoadDropDowns(cancellationToken);

        var model = new StoreViewModel
        {
            IsActive = true
        };

        if (id.HasValue && id.Value > 0)
        {
            var dto = await storeService.GetById(id.Value, cancellationToken);
            if (dto == null)
            {
                TempData["ToastMessage"] = "انبار مورد نظر یافت نشد";
                TempData["ToastType"] = "error";
                return RedirectToAction(nameof(Index));
            }

            model = new StoreViewModel
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

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StoreViewModel model, CancellationToken cancellationToken)
    {
        await LoadDropDowns(cancellationToken);

        if (!ModelState.IsValid)
        {
            TempData["ToastMessage"] = "تمام فیلدهای اجباری باید پر شوند";
            TempData["ToastType"] = "error";
            return View("New", model);
        }

        try
        {
            if (model.StoreId == null || model.StoreId == 0)
            {
                var dto = new StoreCreateDto
                {
                    Code = model.Code,
                    Name = model.Name,
                    TypeId = model.TypeId,
                    FreeVolume = model.FreeVolume ?? 0,
                    Volume = model.Volume ?? 0,
                    Note = model.Note,
                    IsActive = true,
                    RecipientId = model.RecipientId,
                    Year = (int)model.Year
                };

                await storeService.Create(dto, cancellationToken);
            }
            else
            {
                var dto = new StoreEditDto
                {
                    Id = model.StoreId.Value,
                    Code = model.Code,
                    Name = model.Name,
                    TypeId = model.TypeId,
                    FreeVolume = model.FreeVolume ?? 0,
                    Volume = model.Volume ?? 0,
                    Note = model.Note,
                    IsActive = model.IsActive,
                    RecipientId = model.RecipientId,
                    Year = (int) model.Year
                };

                await storeService.Update(dto, cancellationToken);
            }

            TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
            TempData["ToastType"] = "success";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["ToastMessage"] = "خطا در انجام عملیات: " + ex.Message;
            TempData["ToastType"] = "error";
            return View("New", model);
        }
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        return await New(id, cancellationToken);
    }

    #endregion

    #region Toggle Active

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id, CancellationToken cancellationToken)
    {
        try
        {
            await storeService.ToggleActive(id, cancellationToken);
            TempData["ToastMessage"] = "وضعیت انبار با موفقیت تغییر یافت";
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

    private async Task LoadDropDowns(CancellationToken cancellationToken)
    {
        var storeTypes = await baseInfoService.GetByParentIdAsync(56, cancellationToken);
        var recipients = await recipientService.GetActiveRecipients(cancellationToken);

        ViewBag.StoreTypeList = storeTypes is null ? [] : storeTypes
            .Select(x => new SelectListItem
            {
                Text = x.Title,
                Value = x.Id.ToString()
            })
            .ToList();

        ViewBag.RecipientList = recipients is null ? [] : recipients
            .Select(x => new SelectListItem
            {
                Text = x.FullName,
                Value = x.RecipientId.ToString()
            })
            .ToList();

        ViewBag.RecipientListForFilter = recipients is null ? [] : recipients
            .Select(x => new SelectListItem
            {
                Text = x.FullName,
                Value = x.RecipientId.ToString()
            })
            .Prepend(new SelectListItem { Text = "همه", Value = "" })
            .ToList();
    }

    #endregion
}