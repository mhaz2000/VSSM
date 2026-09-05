using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.Item;
using VSSM.Application.DTOs.BaseEntities.ItemDetail;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Application.Shared;
using VSSM.Presentation.Web.Models.Base;
using VSSM.Presentation.Web.Models.Item;

namespace VSSM.Presentation.Web.Controllers;

public class ItemController(
    IItemService itemService,
    IItemDetailsService itemDetailsService,
    IBaseInfoService baseInfoService) : BaseController
{
    private const int VaccineClassId = 54;
    private const int DiluentClassId = 55;
    private const int ItemClassId = 53;


    public async Task<IActionResult> Index(
        CancellationToken cancellationToken,
        string? search = null,
        int page = 1,
        int pageSize = 10,
        string sort = "Id",
        string direction = "desc",
        string? status = null)
    {
        var paged = await itemService.GetPagedList(
            search, page, pageSize, sort, direction, status, cancellationToken);

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
            Columns = new List<GridColumn>
            {
            new() { Header = "کد",           PropertyName = nameof(ItemListDto.Code),        Sortable = true  },
            new() { Header = "نام کامل",      PropertyName = nameof(ItemListDto.FullName),    Sortable = true  },
            new() { Header = "حلال",          PropertyName = nameof(ItemListDto.DiluentName), Sortable = false },
            new() { Header = "ترتیب نمایش",  PropertyName = nameof(ItemListDto.OrderInList), Sortable = true  },
            new() { Header = "وضعیت",         PropertyName = nameof(ItemListDto.IsActive),    Sortable = false, IsBadgeColumn = true,
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
                Title           = "ویرایش",
                Controller      = "Item",
                Action          = "Edit",
                RouteIdProperty = nameof(ItemListDto.ItemId),
                IconClass       = "fa fa-edit",
            },
            new()
            {
                   Title           = "حذف",
                   Controller      = "Item",
                   Action          = "Delete",
                   RouteIdProperty = nameof(ItemListDto.ItemId),
                   IconClass       = "fa fa-trash",
                   Method          = "Post"
            }
            }
        };

        return View(model);
    }

    public async Task<IActionResult> New(CancellationToken cancellationToken = default)
    {
        await PrepareDropdown(cancellationToken);
        return View(new ItemViewModel { IsActive = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            var (success, message) = await itemService.Delete(id, cancellationToken);

            TempData["ToastMessage"] = message;
            TempData["ToastType"] = success ? "success" : "error";
        }
        catch (Exception ex)
        {
            TempData["ToastMessage"] = "خطا در حذف کالا: " + ex.Message;
            TempData["ToastType"] = "error";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Create(ItemViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ToastMessage"] = "لطفاً تمام فیلدهای اجباری را پر کنید";
            TempData["ToastType"] = "error";

            await PrepareDropdown(cancellationToken);
            return View("New", model);  
        }

        var dto = MapToDto(model);

        if (model.ItemId is null or 0)
            await itemService.Create(dto, cancellationToken);
        else
            await itemService.Update(dto, cancellationToken);

        TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
        TempData["ToastType"] = "success";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var item = await itemService.GetById(id, cancellationToken);
        if (item == null) return NotFound();

        var model = new ItemViewModel
        {
            ItemId = item.ItemId,
            ItemClassId = item.ItemClassId.GetValueOrDefault(),
            Code = item.Code,
            FullName = item.FullName,
            IsActive = item.IsActive.GetValueOrDefault(),
            OrderInList = item.OrderId.GetValueOrDefault(),
            Volume = item.Volume.GetValueOrDefault(),
            RequiresDiluent = item.RequiresDiluent.GetValueOrDefault(),
            UserBelowZeroStore = item.UserBelowZeroStore.GetValueOrDefault(),
            UseOrdinaryStore = item.UseOrdinaryStore.GetValueOrDefault(),
            UnitPrice = item.UnitPrice.GetValueOrDefault(),
            UniCode = item.UniCode,
            DayToWarmExpire = item.DayToWarmExpire.GetValueOrDefault(),
            DiluentId = item.DiluentId.GetValueOrDefault(),
            UserAboveZeroStore = item.UserAboveZeroStore.GetValueOrDefault(),
        };

        await PrepareDropdown(cancellationToken);
        return View("New", model);
    }

    [HttpGet("GetItems/{classItemId:required:int}")]
    public async Task<IActionResult> GetItems(int classItemId)
    {
        var items = await itemService.GetItemsByClassAsync(classItemId);
        return Json(new ApiResult<List<ItemListDto>>
        {
            Success = true,
            Message = string.Empty,
            Result = items
        });
    }

    [HttpGet("GetItemDetails/{itemId:required:int}")]
    public async Task<IActionResult> GetItemDetails(int itemId)
    {
        var itemDetails = await itemDetailsService
            .GetItemDetailsAsync(itemId, CurrentUserRecipientId!.Value, CancellationToken.None);

        return Json(new ApiResult<List<ToDispatchItemDetailDto>>
        {
            Success = true,
            Message = string.Empty,
            Result = itemDetails
        });
    }

    private async Task PrepareDropdown(CancellationToken cancellationToken)
    {

        ViewBag.ItemClass = await baseInfoService.GetByParentIdAsync(ItemClassId);

        ViewBag.DiluentDropDown = await itemService.GetItemsByClassAsync(DiluentClassId);
    }

    private static ItemCreateDto MapToDto(ItemViewModel model) => new()
    {
        ItemId = model.ItemId.GetValueOrDefault(),
        FkClassId = model.ItemClassId,
        Code = model.Code,
        FullName = model.FullName,
        IsActive = model.IsActive,
        OrderId = model.OrderInList,
        DayToWarmExpire = model.DayToWarmExpire,
        DiluentId = model.DiluentId,
        RequiresDiluent = model.RequiresDiluent,
        UniCode = model.UniCode,
        UnitPrice = model.UnitPrice,
        UseOrdinaryStore = model.UseOrdinaryStore,
        UserAboveZeroStore = model.UserAboveZeroStore,
        UserBelowZeroStore = model.UserBelowZeroStore,
        Volume = model.Volume
    };
}