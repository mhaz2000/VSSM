using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using VSSM.Application.DTOs.BaseEntities.Reports;
using VSSM.Application.DTOs.BaseEntities.Store;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.Base;

namespace VSSM.Presentation.Web.Controllers
{
    public class StoreReportController(
        IStoreService _storeService,
        IRecipientService _recipientService,
        IBaseInfoService _baseInfoService,
        IFiscalPeriodService _fiscalPeriodService,
        IMemoryCache _cache)
        : BaseController
    {
        private const int StoreTypesParentId = 56;

        #region StoreEquipment

        public async Task<IActionResult> StoreEquipment(CancellationToken cancellationToken = default)
        {
            ViewBag.Recipients = await _recipientService.GetActiveRecipientChildren(
                CurrentUserRecipientId.GetValueOrDefault(), cancellationToken);

            ViewBag.TypeList = await _baseInfoService.GetByParentIdAsync(
                StoreTypesParentId, cancellationToken);

            var fiscalPeriods = await _fiscalPeriodService.GetAllAsListAsync(cancellationToken);

            var firstYear = fiscalPeriods
                .Where(t => t.StartDate is not null)
                .OrderBy(t => t.StartDate!.Value.Year)
                .FirstOrDefault()?.StartDate?.Year ?? 0;

            var lastYear = fiscalPeriods
                .Where(t => t.EndTime is not null)
                .OrderByDescending(t => t.EndTime!.Value.Year)
                .FirstOrDefault()?.EndTime?.Year ?? 0;

            ViewBag.Years = Enumerable
                .Range(firstYear, lastYear - firstYear + 1)
                .ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StoreEquipmentReport(
            StoreEquipmentQuery inputModel,
            int pageNumber = 1,
            int pageSize = 20,
            string sort = "StoreName",
            string direction = "asc",
            string? reportId = null,
            CancellationToken cancellationToken = default)
        {
            PrepareCachedStoreEquipmentReport(
                ref inputModel, ref reportId, pageSize, sort, direction);

            var (data, count) = await _storeService.GetStoreEquipmentReport(
                CurrentUserRecipientId.GetValueOrDefault(),
                inputModel.RecipientId,
                inputModel.StoreTypeId,
                inputModel.VolumeStart,
                inputModel.VolumeEnd,
                inputModel.FreeVolumeStart,
                inputModel.FreeVolumeEnd,
                inputModel.YearStart,
                inputModel.YearEnd,
                inputModel.IsActive,
                pageNumber,
                pageSize,
                sort,
                direction,
                cancellationToken);

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
                    new() { Header = "نام انبار",   PropertyName = nameof(StoreEquipmentReportDto.StoreName) },
                    new() { Header = "نوع انبار",   PropertyName = nameof(StoreEquipmentReportDto.StoreType) },
                    new() { Header = "حجم (لیتر)",  PropertyName = nameof(StoreEquipmentReportDto.Capacity) },
                    new() { Header = "حجم کلی فضا", PropertyName = nameof(StoreEquipmentReportDto.TotalCapacity) },
                    new() { Header = "سال",         PropertyName = nameof(StoreEquipmentReportDto.Year) },
                    new() { Header = "وضعیت",       PropertyName = nameof(StoreEquipmentReportDto.IsActive) }
                }
            };

            return View("StoreEquipmentReport", model);
        }

        private void PrepareCachedStoreEquipmentReport(
            ref StoreEquipmentQuery inputModel,
            ref string? reportId,
            int pageSize,
            string sort,
            string direction)
        {
            if (!string.IsNullOrEmpty(reportId))
            {
                var cached = _cache.Get<StoreEquipmentQuery>(reportId);
                if (cached is not null)
                {
                    inputModel = cached;
                    _cache.Remove(reportId);
                }
            }
            else
            {
                reportId = Guid.NewGuid().ToString("N");
            }

            ViewBag.ReportId = reportId;
            ViewBag.AdditionalParam =
                $"reportId={reportId}" +
                $"&pageSize={pageSize}" +
                $"&sort={Uri.EscapeDataString(sort)}" +
                $"&direction={direction}";

            _cache.Set(reportId, inputModel, TimeSpan.FromDays(1));
        }

        #endregion
    }
}