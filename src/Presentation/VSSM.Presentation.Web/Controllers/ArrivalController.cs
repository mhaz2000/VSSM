using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VSSM.Application.DTOs.Arrivals;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Domain.Enums;
using VSSM.Presentation.Web.Models.Arrival;

namespace VSSM.Presentation.Web.Controllers
{
    public class ArrivalController : Controller
    {
        private readonly IArrivalService _arrivalService;
        private readonly ISourceService _sourceService;
        private readonly IFiscalPeriodService _fiscalService;
        private readonly IItemService _itemService;
        private readonly IStoreService _storeService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IDosePerVialService _dosePerVialService;

        public ArrivalController(
            IArrivalService arrivalService,
            ISourceService sourceService,
            IFiscalPeriodService fiscalService,
            IItemService itemService,
            IStoreService storeService,
            IManufacturerService manufacturerService,
            IDosePerVialService dosePerVialService)
        {
            _arrivalService = arrivalService;
            _sourceService = sourceService;
            _fiscalService = fiscalService;
            _itemService = itemService;
            _storeService = storeService;
            _manufacturerService = manufacturerService;
            _dosePerVialService = dosePerVialService;
        }

        // Index
        public async Task<IActionResult> Index(
            string search,
            ArrivalStatusFilter status = ArrivalStatusFilter.Draft,
            int page = 1,
            int pageSize = 10,
            CancellationToken token = default)
        {
            var list = await _arrivalService.GetList(search, status, page, pageSize, token);

            ViewBag.Status = status;
            ViewBag.Search = search;

            return View(list);
        }

        // GET New/Edit
        public async Task<IActionResult> New(int? id, CancellationToken token)
        {
            await LoadDropDowns(token);

            ArrivalViewModel model;

            if (!id.HasValue || id.Value == 0)
            {
                model = new ArrivalViewModel
                {
                    DateString = DateTime.Today.ToString("yyyy-MM-dd"), // برای datepicker انگلیسی
                    IsDrafted = true
                };
            }
            else
            {
                var dto = await _arrivalService.GetById(id.Value, token);
                if (dto == null)
                    return NotFound();

                model = new ArrivalViewModel
                {
                    ArrivalId = dto.ArrivalId,
                    DateString = dto.DateString,
                    ReportNumber = dto.ReportNumber,
                    BiddingNumber = dto.BiddingNumber,
                    SourceId = dto.SourceId,
                    FiscalPeriodId = dto.FiscalPeriodId,
                    PackedVolume = dto.PackedVolume,
                    TotalWeight = dto.TotalWeight,
                    TotalVolume = dto.TotalVolume,
                    Note = dto.Note,
                    IsConfirmed = dto.IsConfirmed ?? false,
                    IsDrafted = dto.IsDrafted ?? false,
                    IsCancelled = dto.IsCancelled ?? false,
                    Lines = dto.Lines.Select(l => new ArrivalLineViewModel
                    {
                        ArrivalLineId = l.ArrivalLineId,
                        ItemId = l.ItemId,
                        ManufacturerId = l.ManufacturerId,
                        StoreId = l.StoreId,
                        DosePerVialId = l.DosePerVialId,
                        NumVials = l.NumVials,
                        Quantity = l.Quantity,
                        UnitVolume = l.UnitVolume,
                        TotalVolume = l.TotalVolume,
                        BatchNum = l.BatchNum,
                        ExpiryDateString = l.ExpiryDateString,
                        ProductionDateString = l.ProductionDateString,
                        Note = l.Note
                    }).ToList()
                };
            }

            return View("New", model);
        }

        // POST Save (Draft/Confirm)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ArrivalViewModel model,
            string submit, // "confirm" or "draft"
            CancellationToken token)
        {
            await LoadDropDowns(token);

            if (!ModelState.IsValid)
                return View("New", model);

            var dto = new ArrivalCreateEditDto
            {
                ArrivalId = model.ArrivalId,
                DateString = model.DateString,
                ReportNumber = model.ReportNumber,
                BiddingNumber = model.BiddingNumber,
                SourceId = model.SourceId,
                FiscalPeriodId = model.FiscalPeriodId,
                PackedVolume = model.PackedVolume,
                TotalWeight = model.TotalWeight,
                TotalVolume = model.TotalVolume,
                Note = model.Note,
                SaveAsDraft = submit == "draft",
                Confirm = submit == "confirm",
                Lines = model.Lines.Select(l => new ArrivalLineCreateEditDto
                {
                    ArrivalLineId = l.ArrivalLineId,
                    ItemId = l.ItemId,
                    ManufacturerId = l.ManufacturerId,
                    StoreId = l.StoreId,
                    DosePerVialId = l.DosePerVialId,
                    NumVials = l.NumVials,
                    Quantity = l.Quantity,
                    UnitVolume = l.UnitVolume,
                    TotalVolume = l.TotalVolume,
                    BatchNum = l.BatchNum,
                    Note = l.Note,
                    ExpiryDateString = l.ExpiryDateString,
                    ProductionDateString = l.ProductionDateString
                }).ToList()
            };

            var id = await _arrivalService.Save(dto, token);

            TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
            TempData["ToastType"] = "success";

            return RedirectToAction("New", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken token)
        {
            await _arrivalService.Delete(id, token);

            TempData["ToastMessage"] = "ورود حذف شد";
            TempData["ToastType"] = "success";

            return RedirectToAction("Index");
        }

        private async Task LoadDropDowns(CancellationToken token)
        {
            ViewBag.SourceList = (await _sourceService.GetAll(token))
                .Select(x => new SelectListItem
                {
                    Value = x.SourceId.ToString(),
                    Text = x.FullName
                }).ToList();

            ViewBag.FiscalPeriodList = (await _fiscalService.GetActives(token))
                .Select(x => new SelectListItem
                {
                    Value = x.FiscalPeriodId.ToString(),
                    Text = x.FullName
                }).ToList();

            ViewBag.ItemList = (await _itemService.GetActives(token))
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.FullName
                }).ToList();

            ViewBag.StoreList = (await _storeService.GetAll(token))
                .Select(x => new SelectListItem
                {
                    Value = x.StoreId.ToString(),
                    Text = x.FullName
                }).ToList();

            ViewBag.ManufacturerList = (await _manufacturerService.GetActives(token))
                .Select(x => new SelectListItem
                {
                    Value = x.ManufacturerId.ToString(),
                    Text = x.FullName
                }).ToList();

            ViewBag.DosePerVialList = (await _dosePerVialService.GetAll(token))
                .Select(x => new SelectListItem
                {
                    Value = x.DosePerVialId.ToString(),
                    Text = x.NumDosePerVial.ToString()
                }).ToList();
        }
    }
}
