using Microsoft.AspNetCore.Mvc;
using System.Threading;
using VSSM.Application.DTOs.BaseEntities.Item;
using VSSM.Application.DTOs.BaseEntities.ItemDetail;
using VSSM.Application.DTOs.BaseEntities.Reports;
using VSSM.Application.DTOs.Reports;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Application.Services.BaseEntities;
using VSSM.Application.Shared;
using VSSM.Presentation.Web.Models.Base;

namespace VSSM.Presentation.Web.Controllers
{
    public class CurrentStockReportController(IReportService _reportService,
        IItemClassService _itemClassService,
        IItemService _itemService,
        IStoreService _storeService,
        ISourceService _sourceService,
        IRecipientService _recipientService,
        IItemDetailsService _itemDetailsService,
        IFiscalPeriodService _fiscalPeriodService)
        : BaseController
    {

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
            CancellationToken cancellationToken = default)
        {

            var (data, count) = await _reportService.GetOutOfStockItems(
                CurrentUserRecipientId.Value,
                itemClassId,
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
            CancellationToken cancellationToken = default)
        {
            var itemDetail = await _reportService.GetItemIndicatorDetail(CurrentUserRecipientId.Value, itemId, batchNumber);
            
            itemDetail.CurrentStock = (await _itemDetailsService.GetItemBatchNumbers(itemId.Value, CurrentUserRecipientId.Value, CancellationToken.None))
                .FirstOrDefault(t=> t.BatchNumber == batchNumber)?.CurrentStock ?? 0;

            ViewBag.ItemDetail = itemDetail;

            var (data, count) = await _reportService.GetItemIndicator(
                CurrentUserRecipientId.Value,
                itemId,
                batchNumber,
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
