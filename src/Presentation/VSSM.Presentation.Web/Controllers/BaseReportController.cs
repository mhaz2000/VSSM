using Microsoft.AspNetCore.Mvc;
using VSSM.Presentation.Web.Models.Base;
using VSSM.Presentation.Web.Models.Base.Reports;

namespace VSSM.Presentation.Web.Controllers;

public abstract class BaseReportController : BaseController
{
    protected abstract ReportConfig GetConfig();
    protected abstract Task<object> GetReportData(ReportRequest request);

    public IActionResult Index()
    {
        var config = GetConfig();
        return View("~/Views/Shared/ReportIndex.cshtml", config);
    }

    [HttpPost]
    public async Task<IActionResult> Generate([FromForm] ReportRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var data = await GetReportData(request);
        return PartialView("~/Views/Shared/ReportResult.cshtml", data);
    }
}