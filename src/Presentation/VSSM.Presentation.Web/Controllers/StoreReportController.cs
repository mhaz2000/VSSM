using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuGet.Common;
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
    public class StoreReportController(IReportService _reportService,
        IStoreTypeService _storeTypeService,
        IRecipientService _recipientService,
        IFiscalPeriodService _fiscalPeriodService)
        : BaseController
    {



        public async Task<IActionResult> StoreEquipment(CancellationToken cancellationToken = default)
        {

            var recipients = await _recipientService.GetActiveRecipientChildren(CurrentUserRecipientId.Value, cancellationToken);
            ViewBag.Recipients = recipients;

            ViewBag.TypeList = (await _storeTypeService.GetAll(cancellationToken))
                .Select(x => new SelectListItem
                {
                    Value = x.StoreTypeId.ToString(),
                    Text = x.FullName
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
            bool isActive,
            int pageNumber = 1,
            int pageSize = 20,
            string sort = "Recipient",
            string direction = "asc",
            CancellationToken cancellationToken = default)
        {
            return null;
        }
    }
}
