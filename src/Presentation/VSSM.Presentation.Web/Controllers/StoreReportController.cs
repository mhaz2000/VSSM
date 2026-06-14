using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VSSM.Application.DTOs.BaseEntities.Reports;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.Base;

namespace VSSM.Presentation.Web.Controllers
{
    public class StoreReportController(IStoreService _storeService,
        IRecipientService _recipientService,
        IBaseInfoService _baseInfoService,
        IFiscalPeriodService _fiscalPeriodService)
        : BaseController
    {
        public async Task<IActionResult> StoreEquipment(CancellationToken cancellationToken = default)
        {

            var recipients = await _recipientService.GetActiveRecipientChildren(CurrentUserRecipientId.Value, cancellationToken);
            ViewBag.Recipients = recipients;

            ViewBag.TypeList = (await _baseInfoService.GetByParentIdAsync(56, cancellationToken))
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Title
                })
                .ToList();

            var fiscalPeriods = await _fiscalPeriodService.GetAllAsListAsync(cancellationToken);
            List<int> years = new List<int>();
            var firstYear = fiscalPeriods.Where(t => t.StartDate is not null).OrderBy(t => t.StartDate.Value.Year).FirstOrDefault()?.StartDate?.Year ?? 0;
            var lastYear = fiscalPeriods.Where(t => t.EndTime is not null).OrderByDescending(t => t.EndTime.Value.Year).FirstOrDefault()?.EndTime?.Year ?? 0;

            years = Enumerable.Range(firstYear, lastYear - firstYear + 1).ToList();

            ViewBag.Years = years.Select(x => new SelectListItem
            {
                Value = x.ToString(),
                Text = x.ToString()
            }).ToList();



            return View();
        }

        public async Task<IActionResult> StoreEquipmentReport(
            int? recipientId,
            int? storeTypeId,
            int? volumeStart,
            int? volumeEnd,
            int? freeVolumeStart,
            int? freeVolumeEnd,
            int? yearStart,
            int? yearEnd,
            int? isActive,
            int pageNumber = 1,
            int pageSize = 20,
            string sort = "Recipient",
            string direction = "asc",
            CancellationToken cancellationToken = default)
        {

            var (data, count) = await _storeService.GetStoreEquipmentReport(CurrentUserRecipientId.Value,
                    recipientId,
                    storeTypeId,
                    volumeStart,
                    volumeEnd,
                    freeVolumeStart,
                    freeVolumeEnd,
                    yearStart,
                    yearEnd,
                    isActive,
                    pageNumber,
                    pageSize,
                    sort,
                    direction,
                    cancellationToken
                );

            var model = new GridViewModel
            {
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
                    new() { Header = "نام انبار", PropertyName = nameof(StoreEquipmentDto.StoreName) },
                    new() { Header = "نوع انبار", PropertyName = nameof(StoreEquipmentDto.StoreType) },
                    new() { Header = "حجم(لیتر)", PropertyName = nameof(StoreEquipmentDto.Capacity) },
                    new() { Header = "حجم کلی فضا", PropertyName = nameof(StoreEquipmentDto.TotalCapacity) },
                    new() { Header = "سال", PropertyName = nameof(StoreEquipmentDto.Year) },
                    new() { Header = "وضعیت", PropertyName = nameof(StoreEquipmentDto.IsActive) }
                }
            };

            return View(model);
        }
    }
}
