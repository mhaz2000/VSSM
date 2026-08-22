using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.Warnings;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.Base;

namespace VSSM.Presentation.Web.Controllers
{
    
    public class WarningsController(IWarningService warningService) : BaseController
    {
        public async Task<IActionResult> CloseToExpirationItemsIndex(
            int pageNumber = 1,
            int pageSize = 10,
            string sort = nameof(CloseToExpirationDto.ItemName),
            string direction = "asc",
            string search = "",
            CancellationToken cancellationToken = default)
        {
            var (data, count) = await warningService.GetCloseToExpirationAsync(
                search,
                pageNumber,
                pageSize,
                sort,
                direction,
                CurrentUserRecipientId.Value,
                cancellationToken);

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
            new()
            {
                Header = "کالا",
                PropertyName = nameof(CloseToExpirationDto.ItemName),
                Sortable = true
            },
            new()
            {
                Header = "تعداد",
                PropertyName = nameof(CloseToExpirationDto.Quantity),
                IsAmount = true,
                Sortable = true
            },
            new()
            {
                Header = "تاریخ انقضا",
                PropertyName = nameof(CloseToExpirationDto.ExpiredDateString)
            },
            new()
            {
                Header = "مانده تا انقضا",
                PropertyName = nameof(CloseToExpirationDto.DaysToExpired)
            },
            new()
            {
                Header = "سازنده",
                PropertyName = nameof(CloseToExpirationDto.Manufacturer)
            },
            new()
            {
                Header = "سری ساخت",
                PropertyName = nameof(CloseToExpirationDto.BatchNumber)
            },
            new()
            {
                Header = "مرحله VVM",
                PropertyName = nameof(CloseToExpirationDto.VVMStage)
            },
            new()
            {
                Header = "انبار",
                PropertyName = nameof(CloseToExpirationDto.StockName)
            }
        }
            };

            return View(model);
        }

        public async Task<IActionResult> LowItemsMinIndex(
            int pageNumber = 1,
            int pageSize = 10,
            string sort = "ItemName",
            string direction = "asc",
            string search = "", CancellationToken token = default)
        {
            var (data, count) = await warningService.GetLowItemsMinAsync(search, pageNumber, pageSize, sort, direction, CurrentUserRecipientId.Value, token);

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
                    new() { Header = "کالا", PropertyName = nameof(HighItemsMaxDto.ItemName),  Sortable = true },
                    new() { Header = "تعداد", PropertyName = nameof(HighItemsMaxDto.Quantity),  IsAmount = true, Sortable = true },
                    new() { Header = "حداقل موجودی", PropertyName = nameof(HighItemsMaxDto.MinQuantity), IsAmount = true, Sortable = false },
                    new() { Header = "حداکثر موجودی", PropertyName = nameof(HighItemsMaxDto.MaxQuantity), IsAmount = true, Sortable = false },
                    new() { Header = nameof(HighItemsMaxDto.Difference), PropertyName = nameof(HighItemsMaxDto.Difference), IsAmount = true, Sortable = false },
                    new() { Header = "سطح درخواست", PropertyName = nameof(HighItemsMaxDto.OrderLevel), IsAmount = true, Sortable = false },
                }
            };


            return View(model);
        }

        public async Task<IActionResult> HighItemsMaxIndex(
            int pageNumber = 1,
            int pageSize = 10,
            string sort = "ItemName",
            string direction = "asc",
            string search = "", CancellationToken token = default)
        {
            var (data, count) = await warningService.GetHighItemsMaxAsync(search, pageNumber, pageSize, sort, direction, CurrentUserRecipientId.Value, token);

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
                    new() { Header = "کالا", PropertyName = nameof(HighItemsMaxDto.ItemName), Sortable = true },
                    new() { Header = "تعداد", PropertyName = nameof(HighItemsMaxDto.Quantity), IsAmount = true, Sortable = true },
                    new() { Header = "حداقل موجودی", PropertyName = nameof(HighItemsMaxDto.MinQuantity), IsAmount = true, Sortable = false },
                    new() { Header = "حداکثر موجودی", PropertyName = nameof(HighItemsMaxDto.MaxQuantity), IsAmount = true, Sortable = false },
                    new() { Header = nameof(HighItemsMaxDto.Difference), PropertyName = nameof(HighItemsMaxDto.Difference), IsAmount = true, Sortable = false },
                    new() { Header = "سطح درخواست", PropertyName = nameof(HighItemsMaxDto.OrderLevel), IsAmount = true, Sortable = false },
                }
            };


            return View(model);
        }

    }
}
