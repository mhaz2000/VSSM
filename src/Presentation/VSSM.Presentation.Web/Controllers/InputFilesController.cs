using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
using VSSM.Application.DTOs.BaseEntities.Dispatches;
using VSSM.Application.DTOs.BaseEntities.OrderInfo;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Domain.Enums;
using VSSM.Presentation.Web.Models.Base;


namespace VSSM.Presentation.Web.Controllers;

public class InputFilesController(IOrderInfoService _orderInfoService) : BaseController
{
    [HttpGet]
    public async Task<ActionResult> Index(CancellationToken cancellationToken, string? search, int pageNumber = 1,
        int pageSize = 10, string sort = "Id", string direction = "desc")
    {
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
                    new() { Header = "عملیات", IsActionColumn = true}
                },
            Actions = new List<GridAction>
                {
                    new()
                    {
                        DynamicTitle = item =>
                        {
                            var row = (OrderInfoDto)item;

                            if(row.Status == OrderInfoStatus.READ.GetHashCode())
                                return "حذف";
                            if(row.Status == OrderInfoStatus.HAS_ERROR.GetHashCode() || row.Status ==  OrderInfoStatus.APPLY.GetHashCode())
                                return "فایل خطا";

                            return "تایید نهایی";
                        },
                        Controller = "InputFiles",
                        Action = "Remove",
                        RouteIdProperty = nameof(DispatchListDto.Id),
                        LinkClass = "",
                        DynamicIconStyle= item =>
                        {
                            var row = (OrderInfoDto)item;

                            if(row.Status == OrderInfoStatus.READ.GetHashCode())
                                return "color: red;";
                            if(row.Status == OrderInfoStatus.HAS_ERROR.GetHashCode() || row.Status ==  OrderInfoStatus.APPLY.GetHashCode())
                                return "color: blue;";

                            return "color: green;";
                        },
                        DynamicIconClass = item =>
                        {
                            var row = (OrderInfoDto)item;

                            if(row.Status == OrderInfoStatus.READ.GetHashCode())
                                return "fa fa-trash";
                            if(row.Status == OrderInfoStatus.HAS_ERROR.GetHashCode() || row.Status ==  OrderInfoStatus.APPLY.GetHashCode())
                                return "fa fa-file";

                            return "fa fa-check";
                        },
                    }
                }
        };


        return View(model);
    }

    [HttpPost]
    public async Task<ActionResult> Upload(IFormFile xmlFile)
    {
        XDocument doc;

        using (var stream = xmlFile.OpenReadStream())
        {
            doc = XDocument.Load(stream);
        }

        await _orderInfoService.UploadAsync(doc, xmlFile.FileName, CurrentUserId.Value);

        return RedirectToAction(nameof(Index));
    }
}
