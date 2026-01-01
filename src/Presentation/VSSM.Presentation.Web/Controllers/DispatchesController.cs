using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.Dispatches;
using VSSM.Application.DTOs.BaseEntities.Item;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Application.Shared;

namespace VSSM.Presentation.Web.Controllers
{
    //[Authorize]
    public class DispatchesController : Controller
    {
        private readonly IRecipientService _recipientService;
        private readonly ITransportModeService _transportModeService;
        private readonly IFiscalPeriodService _fiscalPeriodService;
        private readonly IReceivingModeService _receivingModeService;
        private readonly IItemService _itemService;
        private readonly IDispatchService _dispatchService;
        private readonly IItemClassService _itemClassService;
        public DispatchesController(
            IRecipientService recipientService,
            ITransportModeService transportModeService,
            IFiscalPeriodService fiscalPeriodService,
            IReceivingModeService receivingModeService,
            IItemService itemService,
            IDispatchService dispatchService,
            IItemClassService itemClassService)
        {
            _recipientService = recipientService;
            _transportModeService = transportModeService;
            _fiscalPeriodService = fiscalPeriodService;
            _receivingModeService = receivingModeService;
            _itemService = itemService;
            _dispatchService = dispatchService;
            _itemClassService = itemClassService;
        }

        public IActionResult Index()
        {
            return View();
        }


        public async Task<IActionResult> Create(CancellationToken cancellationToken)
        {
            ViewBag.Recipients = await _recipientService.GetActiveRecipients(cancellationToken);
            ViewBag.TransportModes = await _transportModeService.GetActiveTransportModes(cancellationToken);
            ViewBag.FiscalPeriods = await _fiscalPeriodService.GetActives(cancellationToken);
            ViewBag.ReceivingModes = await _receivingModeService.GetActiveReceivingModesAsync(cancellationToken);
            ViewBag.ItemClasses = await _itemClassService.GetActives(cancellationToken);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDispatcheDto createDispatcheDto, CancellationToken cancellationToken)
        {
            await _dispatchService.CreateDispatchAsync(createDispatcheDto, cancellationToken);
            return RedirectToAction(nameof(Index));
        }
    }
}
