using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.Reports;
using VSSM.Application.DTOs.Reports;
using VSSM.Application.Interfaces.Reports;
using VSSM.Presentation.Web.Models.Base;

namespace VSSM.Presentation.Web.Controllers
{
    public class PeripheralReportController : BaseController
    {
        private readonly IPeripheralReportService _peripheralReportService;
        public PeripheralReportController(IPeripheralReportService peripheralReportService)
        {
            _peripheralReportService = peripheralReportService;
        }

        public async Task<IActionResult> ExpiredItemsReport(
            int pageNumber = 1,
            int pageSize = 20,
            string sort = "Item",
            string direction = "asc",
            CancellationToken cancellationToken = default)
        {
            var (data, count) = await _peripheralReportService.ExpiredItemsReport(
                CurrentUserRecipientId.Value,
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
                    new() { Header = "کالا", PropertyName = nameof(ExpiredItemsReportDto.ItemName) },
                    new() { Header = "TotalStock", PropertyName = nameof(ExpiredItemsReportDto.TotalStock) },
                    new() { Header = "سازنده", PropertyName = nameof(ExpiredItemsReportDto.CommercialName) },
                    new() { Header = "تاریخ انقضا", PropertyName = nameof(ExpiredItemsReportDto.ExpiryDate) },
                    new() { Header = "تعداد دوز بر ویال", PropertyName = nameof(ExpiredItemsReportDto.DosePerVialCount) },
                    new() { Header = "تعداد ویال", PropertyName = nameof(ExpiredItemsReportDto.VialCount) },
                    new() { Header = "انبار", PropertyName = nameof(ExpiredItemsReportDto.StoreName) },
                }
            };

            return View(model);

        }
    }
}
