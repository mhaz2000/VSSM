using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using VSSM.Application.DTOs.BaseEntities.Item;
using VSSM.Application.DTOs.BaseEntities.ItemDetail;
using VSSM.Application.DTOs.BaseEntities.Reports;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Application.Services.ExcelGenerators;
using VSSM.Application.Shared;
using VSSM.Presentation.Web.Models.Base;

namespace VSSM.Presentation.Web.Controllers
{
    public class CurrentStockReportController(IReportService _reportService,
        IItemClassService _itemClassService,
        ICurrentStockReportExcelGenerator _reportExcelGenerator,
        IItemService _itemService,
        IStoreService _storeService,
        ISourceService _sourceService,
        IRecipientService _recipientService,
        IMemoryCache _cache,
        IManufacturerService _manufacturerService,
        IVvmtypeService _vvmtypeService,
        IVvmstageService _vvmstageService,
        ICommercialNameService _commercialNameService,
        IItemDetailsService _itemDetailsService,
        IFiscalPeriodService _fiscalPeriodService)
        : BaseController
    {
        #region AvailableQuantity
        public async Task<IActionResult> AvailableQuantity(CancellationToken cancellationToken = default)
        {
            // Load dropdowns
            ViewBag.CommercialNames = await _commercialNameService.GetActives(cancellationToken);
            ViewBag.ItemClasses = await _itemClassService.GetActives(cancellationToken);
            ViewBag.Items = await _itemService.GetActives(cancellationToken);
            ViewBag.Sources = await _sourceService.GetAll(cancellationToken);
            ViewBag.Stores = await _storeService.GetAllWithRecipientId(CurrentUserRecipientId!.Value, cancellationToken);
            ViewBag.Manufacturers = await _manufacturerService.GetActives(cancellationToken);
            ViewBag.Vvmstages = await _vvmstageService.GetActives(cancellationToken);
            ViewBag.Vvmtypes = await _vvmtypeService.GetActives(cancellationToken);

            var recipients = await _recipientService.GetActiveRecipientChildren(CurrentUserRecipientId.Value, cancellationToken);
            ViewBag.Recipients = recipients;

            var firstChainLevel = recipients.Where(t => t.ChainLevel is not null).OrderBy(t => t.ChainLevel).FirstOrDefault()?.ChainLevel ?? 0;
            var lastChainLevel = recipients.Where(t => t.ChainLevel is not null).OrderByDescending(t => t.ChainLevel).FirstOrDefault()?.ChainLevel ?? 0;

            List<int> chainLevels = new List<int>();
            chainLevels = Enumerable.Range(firstChainLevel, lastChainLevel - firstChainLevel + 1).ToList();
            ViewBag.ChainLevels = chainLevels;

            return View(null);
        }

        [HttpPost]
        public async Task<IActionResult> AvailableQuantityReport(AvailableQuantityReportQuery inputModel, string reportType, int pageNumber = 1,
            int pageSize = 20,
            string sort = "ItemName",
            string direction = "asc",
            bool toExcel = false,
            string? reportId = null)
        {
            ViewBag.IsPost = true;
            if (reportType == "detail")
                return await AvailableQuantityDetailReport(inputModel, pageNumber, pageSize, sort, direction, toExcel, reportId);

            if (reportType == "summary")
                return await AvailableQuantitySummaryReport(inputModel, pageNumber, pageSize, sort, direction, toExcel, reportId);

            if (reportType == "summaryWithDosePerVial")
                return await AvailableQuantitySummaryWithDosePerVialReport(inputModel, pageNumber, pageSize, sort, direction, toExcel, reportId);

            if (reportType == "quantitySummary")
                return await AvailableQuantityQuantitySummaryWithDosePerVialReport(inputModel, pageNumber, pageSize, sort, direction, toExcel, reportId);

            return View(null);
        }

        private async Task<IActionResult> AvailableQuantityQuantitySummaryWithDosePerVialReport(
            AvailableQuantityReportQuery inputModel,
            int pageNumber = 1,
            int pageSize = 20,
            string sort = "ItemName",
            string direction = "asc",
            bool toExcel = false,
            string? reportId = null,
            CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(reportId))
            {
                inputModel = _cache.Get<AvailableQuantityReportQuery>(reportId);
                _cache.Remove(reportId);
            }
            else
            {
                reportId = Guid.NewGuid().ToString("N");
            }
            ViewBag.AdditionalParam = $"reportType=quantitySummary&reportId={reportId}";

            _cache.Set<AvailableQuantityReportQuery>(reportId, inputModel, TimeSpan.FromDays(1));

            if (toExcel)
            {
                var excelData = await _reportService.GetAvailableQuantityQuantitySummaryReportAsync(inputModel, CurrentUserRecipientId.Value,
                    pageNumber,
                    pageSize,
                    sort,
                    direction,
                    true,
                    cancellationToken);

                MemoryStream stream = _reportExcelGenerator.AvailableQuantityQuantitySummaryReportExcel(excelData.data);
                return File(stream,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "مقدار موجود - خلاصه گزارش.xlsx");
            }

            await SetStoresAndItemsViewBagAsync(inputModel.StoreId, inputModel.ItemId, cancellationToken);

            var (data, count) = await _reportService.GetAvailableQuantityQuantitySummaryReportAsync(inputModel, CurrentUserRecipientId.Value,
                pageNumber,
                pageSize,
                sort,
                direction,
                false,
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
                    new() { Header = "نوع کالا", PropertyName = nameof(AvailableQuantityQuantitySummaryDto.ItemClass) },
                    new() { Header = "Item Code", PropertyName = nameof(AvailableQuantityQuantitySummaryDto.ItemCode) },
                    new() { Header = "کالا", PropertyName = nameof(AvailableQuantityQuantitySummaryDto.ItemName) },
                    new() { Header = "سری ساخت", PropertyName = nameof(AvailableQuantityQuantitySummaryDto.BatchNumber) },
                    new() { Header = "دوز بر ویال", PropertyName = nameof(AvailableQuantityQuantitySummaryDto.DoesPerVial) },
                    new() { Header = "تاریخ انقضا", PropertyName = nameof(AvailableQuantityQuantitySummaryDto.ExpiryDateText) },
                    new() { Header = "تعداد", PropertyName = nameof(AvailableQuantityQuantitySummaryDto.CurrentStock) },
                }
            };

            return View("AvailableQuantityQuantitySummaryReport", model);
        }

        private async Task<IActionResult> AvailableQuantitySummaryWithDosePerVialReport(
            AvailableQuantityReportQuery inputModel,
            int pageNumber = 1,
            int pageSize = 20,
            string sort = "ItemName",
            string direction = "asc",
            bool toExcel = false,
            string? reportId = null,
            CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(reportId))
            {
                inputModel = _cache.Get<AvailableQuantityReportQuery>(reportId);
                _cache.Remove(reportId);
            }
            else
            {
                reportId = Guid.NewGuid().ToString("N");
            }
            ViewBag.AdditionalParam = $"reportType=summaryWithDosePerVial&reportId={reportId}";

            _cache.Set<AvailableQuantityReportQuery>(reportId, inputModel, TimeSpan.FromDays(1));

            if (toExcel)
            {
                var excelData = await _reportService.GetAvailableQuantitySummaryWithDosePerVialReportAsync(inputModel, CurrentUserRecipientId.Value,
                    pageNumber,
                    pageSize,
                    sort,
                    direction,
                    true,
                    cancellationToken);

                MemoryStream stream = _reportExcelGenerator.AvailableQuantitySummaryWithDosePerVialReportExcel(excelData.data);
                return File(stream,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "مقدار موجود - خلاصه گزارش با دوز بر ویال.xlsx");
            }

            await SetStoresAndItemsViewBagAsync(inputModel.StoreId, inputModel.ItemId, cancellationToken);

            var (data, count) = await _reportService.GetAvailableQuantitySummaryWithDosePerVialReportAsync(inputModel, CurrentUserRecipientId.Value,
                pageNumber,
                pageSize,
                sort,
                direction,
                false,
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
                    new() { Header = "Item Code", PropertyName = nameof(AvailableQuantitySummaryWithDosePerVialDto.ItemCode) },
                    new() { Header = "کالا", PropertyName = nameof(AvailableQuantitySummaryWithDosePerVialDto.ItemName) },
                    new() { Header = "دوز بر ویال", PropertyName = nameof(AvailableQuantitySummaryWithDosePerVialDto.DoesPerVial) },
                    new() { Header = "تعداد", PropertyName = nameof(AvailableQuantitySummaryWithDosePerVialDto.CurrentStock) },
                }
            };

            return View("AvailableQuantitySummaryWithDosePerVialReport", model);
        }

        private async Task<IActionResult> AvailableQuantitySummaryReport(
            AvailableQuantityReportQuery inputModel,
            int pageNumber = 1,
            int pageSize = 20,
            string sort = "ItemName",
            string direction = "asc",
            bool toExcel = false,
            string? reportId = null,
            CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(reportId))
            {
                inputModel = _cache.Get<AvailableQuantityReportQuery>(reportId);
                _cache.Remove(reportId);
            }
            else
            {
                reportId = Guid.NewGuid().ToString("N");
            }
            ViewBag.AdditionalParam = $"reportType=summary&reportId={reportId}";

            _cache.Set<AvailableQuantityReportQuery>(reportId, inputModel, TimeSpan.FromDays(1));

            if (toExcel)
            {
                var excelData = await _reportService.GetAvailableQuantitySummaryReportAsync(inputModel, CurrentUserRecipientId.Value,
                    pageNumber,
                    pageSize,
                    sort,
                    direction,
                    true,
                    cancellationToken);

                MemoryStream stream = _reportExcelGenerator.AvailableQuantitySummaryReportExcel(excelData.data);
                return File(stream,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "مقدار موجود - گزارش خلاصه موجودی.xlsx");
            }

            await SetStoresAndItemsViewBagAsync(inputModel.StoreId, inputModel.ItemId, cancellationToken);

            var (data, count) = await _reportService.GetAvailableQuantitySummaryReportAsync(inputModel, CurrentUserRecipientId.Value,
                pageNumber,
                pageSize,
                sort,
                direction,
                false,
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
                    new() { Header = "Item Code", PropertyName = nameof(AvailableQuantitySummaryDto.ItemCode) },
                    new() { Header = "کالا", PropertyName = nameof(AvailableQuantitySummaryDto.ItemName) },
                    new() { Header = "تعداد", PropertyName = nameof(AvailableQuantitySummaryDto.CurrentStock) },
                }
            };

            return View("AvailableQuantitySummaryReport", model);
        }

        private async Task<IActionResult> AvailableQuantityDetailReport(
            AvailableQuantityReportQuery inputModel,
            int pageNumber = 1,
            int pageSize = 20,
            string sort = "ItemName",
            string direction = "asc",
            bool toExcel = false,
            string? reportId = null,
            CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(reportId))
            {
                inputModel = _cache.Get<AvailableQuantityReportQuery>(reportId);
                _cache.Remove(reportId);
            }
            else
            {
                reportId = Guid.NewGuid().ToString("N");
            }
            ViewBag.AdditionalParam = $"reportType=detail&reportId={reportId}";

            _cache.Set<AvailableQuantityReportQuery>(reportId, inputModel, TimeSpan.FromDays(1));

            if (toExcel)
            {
                var excelData = await _reportService.GetAvailableQuantityDetailReportAsync(inputModel, CurrentUserRecipientId.Value,
                    pageNumber,
                    pageSize,
                    sort,
                    direction,
                    true,
                    cancellationToken);

                MemoryStream stream = _reportExcelGenerator.AvailableQuantityDetailReportExcel(excelData.data);
                return File(stream,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "مقدار موجود - جزئیات گزارش.xlsx");
            }

            await SetStoresAndItemsViewBagAsync(inputModel.StoreId, inputModel.ItemId, cancellationToken);

            var (data, count) = await _reportService.GetAvailableQuantityDetailReportAsync(inputModel, CurrentUserRecipientId.Value,
                pageNumber,
                pageSize,
                sort,
                direction,
                false,
                cancellationToken);


            var model = new GroupedGridViewModel<object, object>
            {
                CurrentSort = sort,
                CurrentDirection = direction,
                Data = new PagedResult<object>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = count
                },
                Groups = data.Select(s => new GroupItem<object, object>() { Details = s.Details.Cast<object>().ToList(), Meta = s.Meta }).ToList(),
                Columns = new List<GridColumn>
                {
                    new() { Header = "سری ساخت", PropertyName = nameof(AvailableQuantityDetailDto.BatchNumber) },
                    new() { Header = "سازنده", PropertyName = nameof(AvailableQuantityDetailDto.ManufacturerName) },
                    new() { Header = "تاریخ انقضا", PropertyName = nameof(AvailableQuantityDetailDto.ShamsiExpiryDate) },
                    new() { Header = "روزهای مانده به انقضا", PropertyName = nameof(AvailableQuantityDetailDto.DaysToBeExpired) },
                    new() { Header = "تعداد", PropertyName = nameof(AvailableQuantityDetailDto.CurrentStock) },
                    new() { Header = "دوز بر ویال", PropertyName = nameof(AvailableQuantityDetailDto.DoesPerVial) },
                    new() { Header = "انبار", PropertyName = nameof(AvailableQuantityDetailDto.Stock) },
                    new() { Header = "گیرنده", PropertyName = nameof(AvailableQuantityDetailDto.Recipient) },
                    new() { Header = "تاریخ تولید", PropertyName = nameof(AvailableQuantityDetailDto.ProductionDateText) },
                    new() { Header = "نام تجاری", PropertyName = nameof(AvailableQuantityDetailDto.CommercialName) },
                    new() { Header = "نوع VVM", PropertyName = nameof(AvailableQuantityDetailDto.Vvmtype) },
                    new() { Header = "مرحله VVM", PropertyName = nameof(AvailableQuantityDetailDto.Vvmstage) },
                }
            };

            return View(model);
        }

        private async Task SetStoresAndItemsViewBagAsync(string inputStoreIds, string inputItemIds, CancellationToken cancellationToken)
        {
            var storeList = string.Empty;
            var stores = await _storeService.GetAllWithRecipientId(CurrentUserRecipientId!.Value, cancellationToken);
            var storeIds = (inputStoreIds ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            if (!stores.Select(s => s.StoreId).Except(storeIds).Any())
                storeList = "کلیه انبار ها";
            else
                storeList = await _storeService.GetStoreNamesAsync(storeIds);

            ViewBag.StoreList = storeList;

            var itemList = string.Empty;
            var items = await _itemService.GetActives(cancellationToken);
            var itemIds = (inputItemIds ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            if (!items.Select(s => s.Id).Except(itemIds).Any())
                itemList = "کلیه کالاها";
            else
                itemList = await _itemService.GetItemNamesAsync(itemIds);

            ViewBag.ItemList = itemList;
        }

        #endregion

        #region PhysicalInventory
        public async Task<IActionResult> PhysicalInventory(CancellationToken cancellationToken = default)
        {
            // Load dropdowns
            ViewBag.ItemClasses = await _itemClassService.GetActives(cancellationToken);
            ViewBag.Items = await _itemService.GetActives(cancellationToken);
            ViewBag.Sources = await _sourceService.GetAll(cancellationToken);
            ViewBag.Stores = await _storeService.GetAllWithRecipientId(CurrentUserRecipientId!.Value, cancellationToken);

            return View(null);
        }

        public async Task<IActionResult> PhysicalInventoryReport(
            long? itemClassId,
            long? itemId,
            long? sourceId,
            long? storeId,
            int pageNumber = 1,
            int pageSize = 20,
            string sort = "ItemName",
            string direction = "asc",
            bool toExcel = false,
            CancellationToken cancellationToken = default)
        {

            var (data, count) = await _reportService.GetInventoryQuantitiesAsync(
                itemClassId,
                itemId,
                sourceId,
                storeId,
                pageNumber,
                pageSize,
                sort,
                direction,
                CurrentUserRecipientId!.Value,
                false,
                cancellationToken
            );

            if (toExcel)
            {
                var excelData = await _reportService.GetInventoryQuantitiesAsync(
                itemClassId,
                itemId,
                sourceId,
                storeId,
                pageNumber,
                pageSize,
                sort,
                direction,
                CurrentUserRecipientId!.Value,
                true,
                cancellationToken
                );

                MemoryStream stream = _reportExcelGenerator.PhysicalInventoryExcel(excelData.data);
                return File(stream,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "PhysicalInventoryReport.xlsx");
            }

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
                    new() { Header = "نوع کالا", PropertyName = nameof(InventoryQuantityReportDto.ItemClass) },
                    new() { Header = "کالا", PropertyName = nameof(InventoryQuantityReportDto.Item) },
                    new() { Header = "سازنده", PropertyName = nameof(InventoryQuantityReportDto.Manufacturer) },
                    new() { Header = "سری ساخت", PropertyName = nameof(InventoryQuantityReportDto.BatchNumber) },
                    new() { Header = "تاریخ انقضا", PropertyName = nameof(InventoryQuantityReportDto.ExpiryDate) },
                    new() { Header = "نام انبار", PropertyName = nameof(InventoryQuantityReportDto.Store) },
                    new() { Header = "تعداد", PropertyName = nameof(InventoryQuantityReportDto.Quantity) },
                }
            };

            return View(model);
        }

        #endregion

        #region NotDispatchedToRecipients
        public async Task<IActionResult> NotDispatchedToRecipients(CancellationToken cancellationToken = default)
        {
            ViewBag.ItemClasses = await _itemClassService.GetActives(cancellationToken);
            ViewBag.FiscalPeriods = await _fiscalPeriodService.GetAllAsListAsync(cancellationToken);

            return View(null);
        }

        public async Task<IActionResult> NotDispatchedToRecipientsReport(
            long? itemClassId,
            long? itemId,
            long? fiscalPeriodId,
            string? fromDate,
            string? toDate,
            int pageNumber = 1,
            int pageSize = 20,
            string sort = "Reciever",
            string direction = "asc",
            bool toExcel = false,
            CancellationToken cancellationToken = default)
        {

            var (data, count) = await _reportService.GetNotDispatchedToRecipientsAsync(
                itemId,
                fiscalPeriodId,
                fromDate,
                toDate,
                pageNumber,
                pageSize,
                sort,
                direction,
                false,
                cancellationToken
            );


            if (toExcel)
            {
                var excelData = await _reportService.GetNotDispatchedToRecipientsAsync(
                    itemId,
                    fiscalPeriodId,
                    fromDate,
                    toDate,
                    pageNumber,
                    pageSize,
                    sort,
                    direction,
                    true,
                    cancellationToken
                );

                MemoryStream stream = _reportExcelGenerator.NotDispatchedToRecipientsExcel(excelData.data);
                return File(stream,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "ZeroDispatchReport.xlsx");
            }

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
                    new() { Header = "گیرنده", PropertyName = nameof(NotDispatchedToRecipientsDto.FullName) },
                }
            };

            return View(model);
        }

        #endregion

        #region ItemForRecipientInYear
        public async Task<IActionResult> ItemForRecipientInYear(CancellationToken cancellationToken = default)
        {
            ViewBag.Items = await _itemService.GetActives(cancellationToken);
            var recipients = await _recipientService.GetActiveRecipientChildren(CurrentUserRecipientId.Value, cancellationToken);
            ViewBag.Recipients = recipients;

            var firstChainLevel = recipients.Where(t => t.ChainLevel is not null).OrderBy(t => t.ChainLevel).FirstOrDefault()?.ChainLevel ?? 0;
            var lastChainLevel = recipients.Where(t => t.ChainLevel is not null).OrderByDescending(t => t.ChainLevel).FirstOrDefault()?.ChainLevel ?? 0;

            List<int> chainLevels = new List<int>();
            chainLevels = Enumerable.Range(firstChainLevel, lastChainLevel - firstChainLevel + 1).ToList();
            ViewBag.ChainLevels = chainLevels;

            var fiscalPeriods = await _fiscalPeriodService.GetAllAsListAsync(cancellationToken);

            List<int> years = new List<int>();

            var firstYear = fiscalPeriods.Where(t => t.StartDate is not null).OrderBy(t => t.StartDate.Value.Year).FirstOrDefault()?.StartDate?.Year ?? 0;
            var lastYear = fiscalPeriods.Where(t => t.EndTime is not null).OrderByDescending(t => t.EndTime.Value.Year).FirstOrDefault()?.EndTime?.Year ?? 0;

            years = Enumerable.Range(firstYear, lastYear - firstYear + 1).ToList();

            ViewBag.Years = years;

            return View(null);
        }

        public async Task<IActionResult> ItemForRecipientInYearReport(
            long? itemId,
            string? year,
            string? level,
            int? recipientId,
            int pageNumber = 1,
            int pageSize = 20,
            string sort = "Reciever",
            string direction = "asc",
            bool toExcel = false,
            CancellationToken cancellationToken = default)
        {

            var (data, count) = await _reportService.GetItemForRecipientInYear(
                CurrentUserRecipientId.Value,
                itemId,
                int.Parse(year),
                string.IsNullOrEmpty(level) ? null : int.Parse(level),
                recipientId,
                pageNumber,
                pageSize,
                sort,
                direction,
                false,
                cancellationToken
            );

            if (toExcel)
            {
                var excelData = await _reportService.GetItemForRecipientInYear(
                    CurrentUserRecipientId.Value,
                    itemId,
                    int.Parse(year),
                    string.IsNullOrEmpty(level) ? null : int.Parse(level),
                    recipientId,
                    pageNumber,
                    pageSize,
                    sort,
                    direction,
                    true,
                    cancellationToken
                );

                MemoryStream stream = _reportExcelGenerator.ItemForRecipientInYearExcel(excelData.data);
                return File(stream,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "گزارش کالا برای گیرندگان در سال.xlsx");
            }

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
                    new() { Header = "گیرنده", PropertyName = nameof(ItemForRecipientInYearDto.Reciever) },
                    new() { Header = "ژانویه", PropertyName = nameof(ItemForRecipientInYearDto.Jan) },
                    new() { Header = "فوریه", PropertyName = nameof(ItemForRecipientInYearDto.Feb) },
                    new() { Header = "مارس", PropertyName = nameof(ItemForRecipientInYearDto.Mar) },
                    new() { Header = "آپریل", PropertyName = nameof(ItemForRecipientInYearDto.Apr) },
                    new() { Header = "می", PropertyName = nameof(ItemForRecipientInYearDto.May) },
                    new() { Header = "ژوئن", PropertyName = nameof(ItemForRecipientInYearDto.Jun) },
                    new() { Header = "جولای", PropertyName = nameof(ItemForRecipientInYearDto.Jul) },
                    new() { Header = "آگوست", PropertyName = nameof(ItemForRecipientInYearDto.Agu) },
                    new() { Header = "سپتامبر", PropertyName = nameof(ItemForRecipientInYearDto.Sep) },
                    new() { Header = "اکتبر", PropertyName = nameof(ItemForRecipientInYearDto.Oct) },
                    new() { Header = "نوامبر", PropertyName = nameof(ItemForRecipientInYearDto.Nov) },
                    new() { Header = "دسامبر", PropertyName = nameof(ItemForRecipientInYearDto.Dec) },
                    new() { Header = "Total Quota", PropertyName = nameof(ItemForRecipientInYearDto.TotalQuota) },
                    new() { Header = "مقدار احتیاج سالانه", PropertyName = nameof(ItemForRecipientInYearDto.YearlyNeed) },
                }
            };

            return View(model);
        }

        #endregion

        #region Inventory History

        public async Task<IActionResult> InventoryHistory(CancellationToken cancellationToken = default)
        {
            ViewBag.Items = await _itemService.GetActives(cancellationToken);
            var recipients = await _recipientService.GetActiveRecipientChildren(CurrentUserRecipientId.Value, cancellationToken);
            ViewBag.Recipients = recipients;

            var firstChainLevel = recipients.Where(t => t.ChainLevel is not null).OrderBy(t => t.ChainLevel).FirstOrDefault()?.ChainLevel ?? 0;
            var lastChainLevel = recipients.Where(t => t.ChainLevel is not null).OrderByDescending(t => t.ChainLevel).FirstOrDefault()?.ChainLevel ?? 0;

            List<int> chainLevels = new List<int>();
            chainLevels = Enumerable.Range(firstChainLevel, lastChainLevel - firstChainLevel + 1).ToList();
            ViewBag.ChainLevels = chainLevels;

            var fiscalPeriods = await _fiscalPeriodService.GetAllAsListAsync(cancellationToken);

            ViewBag.FiscalPeriods = fiscalPeriods;

            return View(null);
        }

        public async Task<IActionResult> InventoryHistoryReport(
        long? itemId,
        int? fiscalPeriodId,
        string? level,
        int? recipientId,
        string? fromDate,
        string? toDate,
        int pageNumber = 1,
        int pageSize = 20,
        string sort = "Reciever",
        string direction = "asc",
        bool toExcel = false,
        CancellationToken cancellationToken = default)
        {

            var (data, count) = await _reportService.GetInventoryHistory(
                CurrentUserRecipientId.Value,
                itemId,
                fiscalPeriodId,
                string.IsNullOrEmpty(level) ? null : int.Parse(level),
                recipientId,
                fromDate,
                toDate,
                pageNumber,
                pageSize,
                sort,
                direction,
                false,
                cancellationToken
            );

            if (toExcel)
            {
                var excelData = await _reportService.GetInventoryHistory(
                CurrentUserRecipientId.Value,
                itemId,
                fiscalPeriodId,
                string.IsNullOrEmpty(level) ? null : int.Parse(level),
                recipientId,
                fromDate,
                toDate,
                pageNumber,
                pageSize,
                sort,
                direction,
                false,
                cancellationToken
            );

                MemoryStream stream = _reportExcelGenerator.InventoryHistoryExcel(excelData.data);
                return File(stream,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "تاریخچه موجودی.xlsx");
            }

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
                    new() { Header = "مقدار در پایان ماه", PropertyName = nameof(InventoryHistoryDto.InStock) },
                    new() { Header = "سال", PropertyName = nameof(InventoryHistoryDto.Year) },
                    new() { Header = "ماه", PropertyName = nameof(InventoryHistoryDto.Month) },
                }
            };

            return View(model);
        }

        #endregion

        #region  OutOfStockItems

        public async Task<IActionResult> OutOfStockItems(CancellationToken cancellationToken = default)
        {
            ViewBag.ItemClasses = await _itemClassService.GetActives(cancellationToken);

            return View(null);
        }

        public async Task<IActionResult> OutOfStockItemsReport(
            int? itemClassId,
            int pageNumber = 1,
            int pageSize = 20,
            string sort = "ItemName",
            string direction = "asc",
            bool toExcel = false,
            CancellationToken cancellationToken = default)
        {

            var (data, count) = await _reportService.GetOutOfStockItems(
                CurrentUserRecipientId.Value,
                itemClassId,
                pageNumber,
                pageSize,
                sort,
                direction,
                false,
                cancellationToken
            );


            if (toExcel)
            {
                var excelData = await _reportService.GetOutOfStockItems(
                    CurrentUserRecipientId.Value,
                    itemClassId,
                    pageNumber,
                    pageSize,
                    sort,
                    direction,
                    true,
                    cancellationToken
                );

                MemoryStream stream = _reportExcelGenerator.OutOfStockItemsExcel(excelData.data);
                return File(stream,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "zeroStock.xlsx");
            }

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
                    new() { Header = "نام کالا", PropertyName = nameof(OutOfStockItemsDto.ItemName) }
                }
            };

            return View(model);
        }

        #endregion

        #region Indicator

        public async Task<IActionResult> Indicator(CancellationToken cancellationToken = default)
        {
            ViewBag.ItemClasses = await _itemClassService.GetActives(cancellationToken);

            return View(null);
        }

        public async Task<IActionResult> IndicatorReport(
            int? itemClassId,
            int? itemId,
            string? batchNumber,
            int pageNumber = 1,
            int pageSize = 20,
            string sort = "Recipient",
            string direction = "asc",
            bool toExcel = false,
            CancellationToken cancellationToken = default)
        {
            var itemDetail = await _reportService.GetItemIndicatorDetail(CurrentUserRecipientId.Value, itemId, batchNumber);

            itemDetail.CurrentStock = (await _itemDetailsService.GetItemBatchNumbers(itemId.Value, CurrentUserRecipientId.Value, CancellationToken.None))
                .FirstOrDefault(t => t.BatchNumber == batchNumber)?.CurrentStock ?? 0;

            ViewBag.ItemDetail = itemDetail;

            var (data, count) = await _reportService.GetItemIndicator(
                CurrentUserRecipientId.Value,
                itemId,
                batchNumber,
                pageNumber,
                pageSize,
                sort,
                direction,
                false,
                cancellationToken
            );

            if (toExcel)
            {
                var excelData = await _reportService.GetItemIndicator(
                    CurrentUserRecipientId.Value,
                    itemId,
                    batchNumber,
                    pageNumber,
                    pageSize,
                    sort,
                    direction,
                    true,
                    cancellationToken
                );

                MemoryStream stream = _reportExcelGenerator.ItemIndicatorExcel(excelData.data);
                return File(stream,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "گزارش اندیکاتور.xlsx");
            }

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
                    new() { Header = "شماره حواله / شماره گزارش", PropertyName = nameof(ItemIndicatorDto.ReportNumber) },
                    new() { Header = "شماره درخواست", PropertyName = nameof(ItemIndicatorDto.RequestNum) },
                    new() { Header = "تاریخ", PropertyName = nameof(ItemIndicatorDto.DateText) },
                    new() { Header = "از / به", PropertyName = nameof(ItemIndicatorDto.Recipient) },
                    new() { Header = "ورودی (دریافتی)", PropertyName = nameof(ItemIndicatorDto.Arrival) },
                    new() { Header = "خروجی (ارسالی)", PropertyName = nameof(ItemIndicatorDto.Dispatch) },
                    new() { Header = "مانده", PropertyName = nameof(ItemIndicatorDto.CurrentStock) },
                    new() { Header = "توضیح", PropertyName = nameof(ItemIndicatorDto.Description) },
                }
            };

            return View(model);
        }


        #endregion

        [HttpGet("GetItemsByClass/{classItemId:required:int}")]
        public async Task<IActionResult> GetItems(int classItemId)
        {
            var apiResult = new ApiResult<List<ItemListDto>>();

            try
            {
                var items = await _itemService.GetItemsByClassId(classItemId);

                apiResult.Success = true;
                apiResult.Message = string.Empty;
                apiResult.Result = items;
            }
            catch (Exception ex)
            {

                throw;
            }

            return Json(apiResult);
        }

        [HttpGet("GetItemBatchNumbers/{itemId:required:int}")]
        public async Task<IActionResult> GetItemBatchNumbers(int itemId)
        {
            var apiResult = new ApiResult<List<ItemBatchNumbersDto>>();

            try
            {
                var itemDetails = await _itemDetailsService.GetItemBatchNumbers(itemId, CurrentUserRecipientId.Value, CancellationToken.None);

                apiResult.Success = true;
                apiResult.Message = string.Empty;
                apiResult.Result = itemDetails;
            }
            catch (Exception ex)
            {
            }

            return Json(apiResult);
        }



    }
}
