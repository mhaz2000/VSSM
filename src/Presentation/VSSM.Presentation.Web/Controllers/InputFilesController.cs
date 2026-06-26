using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
using VSSM.Application.DTOs.BaseEntities.Dispatches;
using VSSM.Application.DTOs.BaseEntities.DosePerVial;
using VSSM.Application.DTOs.BaseEntities.ItemDetail;
using VSSM.Application.DTOs.BaseEntities.OrderInfo;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Application.Services.BaseEntities;
using VSSM.Application.Shared;
using VSSM.Domain.Enums;
using VSSM.Presentation.Web.Models.Base;


namespace VSSM.Presentation.Web.Controllers;

public class InputFilesController(IOrderInfoService _orderInfoService, IItemService _itemService, IBaseInfoService _dosePerVialService) : BaseController
{
    [HttpGet]
    public async Task<ActionResult> Index(CancellationToken cancellationToken, string? search, int pageNumber = 1,
        int pageSize = 10, string sort = "Id", string direction = "desc")
    {

        ViewBag.Items = await _itemService.GetActiveVaccines(cancellationToken);
        ViewBag.DosePerVials = await _dosePerVialService.GetByParentIdAsync(84, cancellationToken);

        var (data, count) = await _orderInfoService.GetPagedAsync(search, pageNumber, pageSize, sort, direction, cancellationToken);

        var model = new GridViewModel
        {
            SearchTerm = search,
            CurrentSort = sort,
            CurrentDirection = direction,
            Data = new PagedResult<object>
            {
                Items = data.Cast<object>().ToList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = count
            },
            Columns = new List<GridColumn>
                {
                    new() { Header = "نام فایل", PropertyName = nameof(OrderInfoDto.FileName),  Sortable = false },
                    new() { Header = "تاریخ ایجاد", PropertyName = nameof(OrderInfoDto.CreationTime), Sortable = false },
                    new() { Header = "ایجاد کننده", PropertyName = nameof(OrderInfoDto.CreatedBy), Sortable = false },
                    new() { Header = "سری ساخت", PropertyName = nameof(OrderInfoDto.BatchNumber), Sortable = false },
                    new() { Header = "تعداد", PropertyName = nameof(OrderInfoDto.Quantity), Sortable = false },
                    new() { Header = "وضعیت", PropertyName = nameof(OrderInfoDto.Status), Sortable = false },
                    new() { Header = "عملیات", IsActionColumn = true}
                },
            Actions = new List<GridAction>
                {
                    new()
                    {
                        DynamicTitle = item =>
                        {
                            var row = (OrderInfoDto)item;

                            if(row.Status == OrderInfoStatus.READ.ToString())
                                return "حذف";

                            return string.Empty;
                        },
                        Controller = "InputFiles",
                        Action = "Remove",
                        Method = "Post",
                        RouteIdProperty = nameof(OrderInfoDto.Id),
                        LinkClass = "",
                        DynamicIconStyle= item =>
                        {
                            var row = (OrderInfoDto)item;

                            if(row.Status == OrderInfoStatus.READ.ToString())
                                return "color: red;";

                            return string.Empty;
                        },
                        DynamicIconClass = item =>
                        {
                            var row = (OrderInfoDto)item;

                            if(row.Status == OrderInfoStatus.READ.ToString())
                                return "fa fa-trash";

                            return string.Empty;
                        },
                    },
                     new()
                    {
                        DynamicTitle = item =>
                        {
                            var row = (OrderInfoDto)item;

                            if(row.Status == OrderInfoStatus.HAS_ERROR.ToString() || row.Status ==  OrderInfoStatus.APPLIED.ToString())
                                return "فایل خطا";

                            return string.Empty;
                        },
                        Controller = "InputFiles",
                        Action = "DownloadErrorFile",
                        RouteIdProperty = nameof(OrderInfoDto.Id),
                        LinkClass = "",
                        DynamicIconStyle= item =>
                        {
                            var row = (OrderInfoDto)item;

                            if(row.Status == OrderInfoStatus.HAS_ERROR.ToString() || row.Status ==  OrderInfoStatus.APPLIED.ToString())
                                return "color: orange;";

                            return string.Empty;
                        },
                        DynamicIconClass = item =>
                        {
                            var row = (OrderInfoDto)item;

                            if(row.Status == OrderInfoStatus.HAS_ERROR.ToString() || row.Status ==  OrderInfoStatus.APPLIED.ToString())
                                return "fa fa-file";

                            return string.Empty;
                        },
                    },
                     new()
                    {
                        DynamicTitle = item =>
                        {
                            var row = (OrderInfoDto)item;

                            if(row.Status == OrderInfoStatus.HAS_ERROR.ToString() || row.Status ==  OrderInfoStatus.IS_VALID.ToString())
                                return "تایید نهایی";

                            return string.Empty;
                        },
                        Controller = "InputFiles",
                        Action = "Confirm",
                        RouteIdProperty = nameof(OrderInfoDto.Id),
                        LinkClass = "",
                        Method = "Post",
                        DynamicIconStyle= item =>
                        {
                            var row = (OrderInfoDto)item;

                            if(row.Status == OrderInfoStatus.HAS_ERROR.ToString() || row.Status ==  OrderInfoStatus.IS_VALID.ToString())
                                return "color: green;";

                            return string.Empty;
                        },
                        DynamicIconClass = item =>
                        {
                            var row = (OrderInfoDto)item;

                            if(row.Status == OrderInfoStatus.HAS_ERROR.ToString() || row.Status ==  OrderInfoStatus.IS_VALID.ToString())
                                return "fa fa-check";

                            return string.Empty;
                        },
                        DynamicSkip = _ =>
                        {
                            return (!(CurrentUserRoleId.HasValue && CurrentUserRoleId == 1)).ToString();
                        }
                    }
                }
        };


        return View(model);
    }

    [HttpPost]
    public async Task<ActionResult> Upload(IFormFile xmlFile, int itemId, int dosePerVialId)
    {
        XDocument doc;

        using (var stream = xmlFile.OpenReadStream())
        {
            doc = XDocument.Load(stream);
        }

        await _orderInfoService.UploadAsync(doc, xmlFile.FileName, itemId, dosePerVialId, CurrentUserId.Value);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<ActionResult> Remove(int id, CancellationToken cancellationToken)
    {
        await _orderInfoService.RemoveAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<ActionResult> Confirm(int id, CancellationToken cancellationToken)
    {
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<ActionResult> DownloadErrorFile(int id, CancellationToken cancellationToken)
    {
        return RedirectToAction(nameof(Index));
    }


    [HttpGet("GetItemDoesPerVials/{itemId}")]
    public async Task<IActionResult> GetItemDoesPerVials(int itemId, CancellationToken token)
    {
        var list = (await _itemService.GetItemDoesPerVialsAsync(itemId, token));
        var apiResult = new ApiResult<List<DosePerVialListDto>>
        {
            Success = true,
            Message = string.Empty,
            Result = list.ToList()
        };

        return Json(apiResult);
    }
}
