using Microsoft.AspNetCore.Mvc;
using VSSM.Application.Interfaces.BaseEntities;
using System.Threading;
using VSSM.Application.DTOs.BaseEntities.Item;
using VSSM.Application.DTOs.BaseEntities.ItemDetail;
using VSSM.Application.DTOs.BaseEntities.Reports;
using VSSM.Application.DTOs.Reports;
using VSSM.Application.Services.BaseEntities;
using VSSM.Application.Shared;
using VSSM.Presentation.Web.Models.Base;

namespace VSSM.Presentation.Web.Controllers
{
    public class StoreReportController(IReportService _reportService,
        IItemClassService _itemClassService,
        IItemService _itemService,
        IStoreService _storeService,
        ISourceService _sourceService,
        IRecipientService _recipientService,
        IItemDetailsService _itemDetailsService,
        IFiscalPeriodService _fiscalPeriodService)
        : BaseController
    {



        public async Task<IActionResult> StoreEquipment(CancellationToken cancellationToken = default)
        {

            var recipients = await _recipientService.GetActiveRecipientChildren(CurrentUserRecipientId.Value, cancellationToken);
            ViewBag.Recipients = recipients;

            var fiscalPeriods = await _fiscalPeriodService.GetAllAsListAsync(cancellationToken);

            List<int> years = new List<int>();

            var firstYear = fiscalPeriods.Where(t => t.StartDate is not null).OrderBy(t => t.StartDate.Value.Year).FirstOrDefault()?.StartDate?.Year ?? 0;
            var lastYear = fiscalPeriods.Where(t => t.EndTime is not null).OrderByDescending(t => t.EndTime.Value.Year).FirstOrDefault()?.EndTime?.Year ?? 0;

            years = Enumerable.Range(firstYear, lastYear - firstYear + 1).ToList();

            ViewBag.Years = years;



            return View();
        }


    }
}
