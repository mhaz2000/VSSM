using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.Dispatches;
using VSSM.Application.DTOs.BaseEntities.Item;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Application.Services.BaseEntities;
using VSSM.Application.Shared;

namespace VSSM.Presentation.Web.Controllers
{
    
    public class DispatchesController : BaseController
    {
        private readonly IRecipientService _recipientService;
        private readonly ITransportModeService _transportModeService;
        private readonly IFiscalPeriodService _fiscalPeriodService;
        private readonly IReceivingModeService _receivingModeService;
        private readonly IStoreService _storeService;
        private readonly IItemService _itemService;
        private readonly IUseService _useService;
        private readonly IDispatchService _dispatchService;
        private readonly IItemClassService _itemClassService;
        private readonly IDosePerVialService _dosePerVialService;
        private readonly IVvmstageService _vvmstageService;
        private readonly IVvmtypeService _vvmtypeService;
        private readonly IManufacturerService _manufacturerService;

        public DispatchesController(
            IManufacturerService manufacturerService,
            IVvmstageService vvmstageService,
            IVvmtypeService vvmtypeService,
            IUseService useService,
            IStoreService storeService,
            IRecipientService recipientService,
            ITransportModeService transportModeService,
            IFiscalPeriodService fiscalPeriodService,
            IReceivingModeService receivingModeService,
            IDosePerVialService dosePerVialService,
            IItemService itemService,
            IDispatchService dispatchService,
            IItemClassService itemClassService)
        {
            _manufacturerService = manufacturerService;
            _vvmstageService = vvmstageService;
            _vvmtypeService = vvmtypeService;
            _recipientService = recipientService;
            _storeService = storeService;
            _transportModeService = transportModeService;
            _fiscalPeriodService = fiscalPeriodService;
            _receivingModeService = receivingModeService;
            _itemService = itemService;
            _dispatchService = dispatchService;
            _itemClassService = itemClassService;
            _dosePerVialService = dosePerVialService;
            _useService = useService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken, string? search,
                                                int page = 1,
                                                int pageSize = 10,
                                                string? status = null,
                                                string sortField = "Orders",
                                                string sortOrder = "desc")
        {
            var dispatches = await _dispatchService.GetPagedDispatchesAsync(search, page, pageSize, sortField, sortOrder, status, CurrentUserRecipientId.Value, cancellationToken);
            return View(dispatches);
        }

        public async Task<IActionResult> ToBeArrivedIndex(CancellationToken cancellationToken, string? search,
                                        int page = 1,
                                        int pageSize = 10,
                                        string? status = null,
                                        string sortField = "Orders",
                                        string sortOrder = "desc")
        {
            var dispatches = await _dispatchService.GetPagedToBeArrivedDispatchesAsync(search, page, pageSize, sortField, sortOrder, status, CurrentUserRecipientId.Value, cancellationToken);
            return View(dispatches);
        }

        public async Task<IActionResult> Confirm(int id, CancellationToken token)
        {
            await _dispatchService.ConfirmDispatchAsync(id, token);

            return RedirectToAction("Index");

        }

        public async Task<IActionResult> Delete(int id, CancellationToken token)
        {
            await _dispatchService.DeleteDispatchAsync(id, token);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> New(CancellationToken cancellationToken)
        {
            ViewBag.Recipients = await _recipientService.GetActiveRecipients(cancellationToken);
            ViewBag.TransportModes = await _transportModeService.GetActiveTransportModes(cancellationToken);
            ViewBag.ReceivingModes = await _receivingModeService.GetActiveReceivingModesAsync(cancellationToken);
            ViewBag.ItemClasses = await _itemClassService.GetActives(cancellationToken);
            ViewBag.VVMStages = await _vvmstageService.GetActives(cancellationToken);
            return View();
        }

        public async Task<IActionResult> Arrive(long id, CancellationToken cancellationToken)
        {
            ViewBag.DispatchLines = await _dispatchService.GetDispatchLinesAsync(id,cancellationToken);
            ViewBag.Stores = ((await _storeService.GetAll(cancellationToken)).Where(store=> store.RecipientId == CurrentUserRecipientId)).ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateDispatchDto createDispatcheDto,
            string submitType,
            CancellationToken cancellationToken)
        {
            createDispatcheDto.SenderId = CurrentUserRecipientId.Value;
            createDispatcheDto.EnteredById = CurrentUserId.Value;


            await _dispatchService.CreateDispatchAsync(createDispatcheDto, submitType, cancellationToken);

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> ArriveSubmit(DispatchArrivedDto arrivedDto, CancellationToken cancellationToken)
        {
            await _dispatchService.DispatchArrivedAsync(arrivedDto, CurrentUserId.Value,CurrentUserRecipientId.Value, cancellationToken);
            return RedirectToAction("ToBeArrivedIndex");
        }
    }
}
