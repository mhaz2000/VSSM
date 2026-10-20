using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.BaseInfo;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.Base;

namespace VSSM.Presentation.Web.Controllers;    

public class BaseInfoController(IBaseInfoService baseInfoService) : BaseController
{
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken, string? search,
        int pageNumber = 1, int pageSize = 10)
    {
        var (data, count) = await baseInfoService
            .GetPagedBaseInfoAsync(search, pageNumber, pageSize, cancellationToken);

        var model = new GridViewModel
        {
            SearchTerm = search,
            Data = new PagedResult<object>
            {
                Items = data.Cast<object>().ToList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = count
            },
            Columns = new List<GridColumn>
            {
                new() { Header = "شناسه سطر", PropertyName = nameof(BaseInfoDto.Id),             Sortable = false },
                new() { Header = "عنوان",     PropertyName = nameof(BaseInfoDto.Title),          Sortable = false },
                new() { Header = "کد",        PropertyName = nameof(BaseInfoDto.Code),           Sortable = false },
                new() { Header = "والد",      PropertyName = nameof(BaseInfoDto.ParentTitle),    Sortable = false },
                new() { Header = "تاریخ بروزرسانی", PropertyName = nameof(BaseInfoDto.UpdatedDateText), Sortable = false },
                new() { Header = "عملیات", IsActionColumn = true }
            },
            Actions = new List<GridAction>
            {
                new() {
                    Title = "افزودن", Controller = "BaseInfo", Action = "Add",
                    RouteIdProperty = nameof(BaseInfoDto.Id), IconClass = "fa fa-plus",
                    OpenModal = true,
                    DynamicSkip = item =>
                    {
                        var row = (BaseInfoDto)item;
                        return row.ParentId != null ? "true" : "false";
                    }
                },
                new() {
                    Title = "ویرایش", Controller = "BaseInfo", Action = "Edit",
                    RouteIdProperty = nameof(BaseInfoDto.Id), IconClass = "fa fa-edit",
                    OpenModal = true
                },
                new() {
                    Title = "حذف", Controller = "BaseInfo", Action = "Remove", Method = "Post",
                    RouteIdProperty = nameof(BaseInfoDto.Id), IconClass = "fa fa-trash"
                }
            }
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int id)
    {
        await baseInfoService.RemoveAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Add(int id)
    {
        var model = new BaseInfoInputDto { ParentId = id };
        var item = await baseInfoService.GetByIdAsync(id);

        if (item.ParentId is not null)
            return NoContent();

        return PartialView("_BaseInfoForm", model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await baseInfoService.GetByIdAsync(id);

        return PartialView("_BaseInfoForm", new BaseInfoInputDto
        {
            BaseInfoId = item.Id,
            ParentId = item.ParentId,
            Order = item.Order,
            Title = item.Title ?? "",
            Code = item.Code,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(BaseInfoInputDto model)
    {
        if (!ModelState.IsValid)
            return PartialView("_BaseInfoForm", model);

        if (!model.BaseInfoId.HasValue || model.BaseInfoId == 0)
            await baseInfoService.AddAsync(model, CurrentUserId.GetValueOrDefault());
        else
            await baseInfoService.UpdateAsync(model, CurrentUserId.GetValueOrDefault());

        TempData["ToastMessage"] = "عملیات با موفقیت شد";
        return RedirectToAction(nameof(Index));
    }
}