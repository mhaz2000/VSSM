using Microsoft.AspNetCore.Mvc;
using VSSM.Application.DTOs.BaseEntities.FiscalPeriod;
using VSSM.Application.Interfaces.BaseEntities;

namespace VSSM.Presentation.Web.Controllers
{
    public class FiscalPeriodController : Controller
    {
        private readonly IFiscalPeriodService _fiscalPeriodService;

        public FiscalPeriodController(IFiscalPeriodService fiscalPeriodService)
        {
            _fiscalPeriodService = fiscalPeriodService;
        }

        // GET: /FiscalPeriod
        public async Task<IActionResult> Index(
            CancellationToken cancellationToken,
            string? search,
            int page = 1,
            int pageSize = 10,
            string sortField = "FiscalPeriodId",
            string sortOrder = "desc")
        {
            var listModel = await _fiscalPeriodService.GetPagedList(
                search, page, pageSize, sortField, sortOrder, cancellationToken);

            return View(listModel);
        }

        // GET: /FiscalPeriod/New
        public IActionResult New()
        {
            var model = new FiscalPeriodDto();
            var today = DateTime.Today;
            var lastDayOfYear = new DateTime(today.Year, 12, 31);
    
            // تنظیم مقدار پیش‌فرض
            model.SuggestedEndDate = lastDayOfYear;
            
            return View(model);
        }

        // POST: /FiscalPeriod/New
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> New(FiscalPeriodDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            await _fiscalPeriodService.Create(dto, cancellationToken);
            return RedirectToAction(nameof(Index));
        }

        // GET: /FiscalPeriod/Edit/5
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var dto = await _fiscalPeriodService.GetById(id, cancellationToken);
            if (dto == null)
                return NotFound();

            return View("New", dto);
        }

        // POST: /FiscalPeriod/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FiscalPeriodDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View("New", dto);
            }

            var updated = await _fiscalPeriodService.Update(dto, cancellationToken);
            if (!updated)
                return NotFound();

            return RedirectToAction(nameof(Index));
        }

        // GET: /FiscalPeriod/Delete/5
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _fiscalPeriodService.Delete(id, cancellationToken);
            return RedirectToAction(nameof(Index));
        }
    }
}
