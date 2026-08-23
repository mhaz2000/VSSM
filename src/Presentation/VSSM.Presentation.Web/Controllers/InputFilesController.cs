using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Xml;
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

public class InputFilesController(
    IOrderInfoService _orderInfoService,
    IItemService _itemService,
    IBaseInfoService _dosePerVialService) : BaseController
{
    [HttpGet]
    public async Task<ActionResult> Index(CancellationToken cancellationToken, string? search, int pageNumber = 1,
        int pageSize = 10, string sort = "Id", string direction = "desc")
    {
        ViewBag.Items = await _itemService.GetActiveVaccines(cancellationToken);
        ViewBag.DosePerVials = await _dosePerVialService.GetByParentIdAsync(84, cancellationToken);

        var (data, count) = await _orderInfoService.GetPagedAsync(search, pageNumber, pageSize, sort, direction, cancellationToken);

        bool isAdmin = User.IsInRole("Admin") || (CurrentUserRoleId.HasValue && CurrentUserRoleId == 1);

        var actions = new List<GridAction>();
        if (isAdmin)
        {
            actions = new List<GridAction>
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
                DynamicIconStyle = item =>
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
                    if(row.Status == OrderInfoStatus.HAS_ERROR.ToString() || row.Status == OrderInfoStatus.APPLIED.ToString())
                        return "فایل خطا";
                    return string.Empty;
                },
                Controller = "InputFiles",
                Action = "DownloadErrorFile",
                RouteIdProperty = nameof(OrderInfoDto.Id),
                LinkClass = "",
                DynamicIconStyle = item =>
                {
                    var row = (OrderInfoDto)item;
                    if(row.Status == OrderInfoStatus.HAS_ERROR.ToString() || row.Status == OrderInfoStatus.APPLIED.ToString())
                        return "color: orange;";
                    return string.Empty;
                },
                DynamicIconClass = item =>
                {
                    var row = (OrderInfoDto)item;
                    if(row.Status == OrderInfoStatus.HAS_ERROR.ToString() || row.Status == OrderInfoStatus.APPLIED.ToString())
                        return "fa fa-file";
                    return string.Empty;
                },
            },
            new()
            {
                DynamicTitle = item =>
                {
                    var row = (OrderInfoDto)item;
                    if(row.Status == OrderInfoStatus.HAS_ERROR.ToString() || row.Status == OrderInfoStatus.IS_VALID.ToString() ||
                        (CurrentUserRoleId.HasValue && CurrentUserRoleId == 1 && row.Status == OrderInfoStatus.READ.ToString()))
                        return "تایید نهایی";
                    return string.Empty;
                },
                Controller = "InputFiles",
                Action = "Confirm",
                RouteIdProperty = nameof(OrderInfoDto.Id),
                LinkClass = "",
                Method = "Post",
                DynamicLinkClass = item =>
                {
                    var row = (OrderInfoDto)item;
                    if(row.Status == OrderInfoStatus.READ.ToString())
                        return "confirm-read";
                    return "";
                },
                DynamicIconStyle = item =>
                {
                    var row = (OrderInfoDto)item;
                    if(row.Status == OrderInfoStatus.HAS_ERROR.ToString() || row.Status == OrderInfoStatus.IS_VALID.ToString() ||
                        (CurrentUserRoleId.HasValue && CurrentUserRoleId == 1 && row.Status == OrderInfoStatus.READ.ToString()))
                        return "color: green;";
                    return string.Empty;
                },
                DynamicIconClass = item =>
                {
                    var row = (OrderInfoDto)item;
                    if(row.Status == OrderInfoStatus.HAS_ERROR.ToString() || row.Status == OrderInfoStatus.IS_VALID.ToString() ||
                        (CurrentUserRoleId.HasValue && CurrentUserRoleId == 1 && row.Status == OrderInfoStatus.READ.ToString()))
                        return "fa fa-check";
                    return string.Empty;
                }
            }
        };
        }

        var columns = new List<GridColumn>
    {
        new() { Header = "نام فایل", PropertyName = nameof(OrderInfoDto.FileName), Sortable = false },
        new() { Header = "تاریخ ایجاد", PropertyName = nameof(OrderInfoDto.CreationTime), Sortable = false },
        new() { Header = "ایجاد کننده", PropertyName = nameof(OrderInfoDto.CreatedBy), Sortable = false },
        new() { Header = "سری ساخت", PropertyName = nameof(OrderInfoDto.BatchNumber), Sortable = false },
        new() { Header = "تعداد", PropertyName = nameof(OrderInfoDto.Quantity), Sortable = false },
        new() { Header = "نام کالا", PropertyName = nameof(OrderInfoDto.ItemName), Sortable = false },
        new() { Header = "دوز بر ویال", PropertyName = nameof(OrderInfoDto.DoseName), Sortable = false },
        new() { Header = "وضعیت", PropertyName = nameof(OrderInfoDto.Status), Sortable = false },
    };
        if (isAdmin)
        {
            columns.Add(new() { Header = "عملیات", IsActionColumn = true });
        }

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
            Columns = columns,
            Actions = actions
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Upload(IFormFile xmlFile, int itemId, int dosePerVialId)
    {
        // [SECURITY - A09/upload]: validate the upload — no type/size checks before meant an
        // arbitrary or oversized file could be posted. Only XML is accepted; XXE is explicitly
        // prohibited via the XmlReader settings.
        if (xmlFile == null || xmlFile.Length == 0)
        {
            TempData["ToastMessage"] = "فایلی انتخاب نشده است.";
            TempData["ToastType"] = "error";
            return RedirectToAction(nameof(Index));
        }

        const long MaxBytes = 10 * 1024 * 1024; // 10 MB
        if (xmlFile.Length > MaxBytes)
        {
            TempData["ToastMessage"] = "حجم فایل بیش از حد مجاز (۱۰ مگابایت) است.";
            TempData["ToastType"] = "error";
            return RedirectToAction(nameof(Index));
        }

        if (!string.Equals(Path.GetExtension(xmlFile.FileName), ".xml", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ToastMessage"] = "فقط فایل‌های XML مجاز هستند.";
            TempData["ToastType"] = "error";
            return RedirectToAction(nameof(Index));
        }

        XDocument doc;
        try
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            };

            using var stream = xmlFile.OpenReadStream();
            using var reader = XmlReader.Create(stream, settings);
            doc = XDocument.Load(reader);
        }
        catch (Exception)
        {
            TempData["ToastMessage"] = "فایل XML معتبر نیست.";
            TempData["ToastType"] = "error";
            return RedirectToAction(nameof(Index));
        }

        await _orderInfoService.UploadAsync(doc, xmlFile.FileName, itemId, dosePerVialId, CurrentUserId.Value);

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Remove(int id, CancellationToken cancellationToken)
    {
        await _orderInfoService.RemoveAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Confirm(int id, CancellationToken cancellationToken)
    {
        await _orderInfoService.ConfirmAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult> DownloadErrorFile(int id, CancellationToken cancellationToken)
    {
        var hasErrors = await _orderInfoService.HasErrorsAsync(id, cancellationToken);
        if (!hasErrors)
        {
            TempData["ToastMessage"] = "فایل مورد نظر فاقد خطا می‌باشد.";
            TempData["ToastType"] = "error";
        }
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
