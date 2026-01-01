using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Application.Interfaces.RequestEntities;
using VSSM.Application.Services.BaseEntities;

namespace VSSM.Presentation.Web.Controllers
{
    [Authorize]
    public class RequestController : BaseController
    {
        private readonly IRequestService _requestService;
        public RequestController(IRequestService requestService)
        {
            _requestService = requestService;
        }
        public async Task<IActionResult> Index(CancellationToken cancellationToken, string? search,
                                                int page = 1,
                                                int pageSize = 10,
                                                string sortField = "Orders",
                                                string sortOrder = "desc")
        {
            var listModel = await _requestService.GetPagedList(search,
                                                                    page,
                                                                    pageSize,
                                                                    sortField,
                                                                    sortOrder,
                                                                    cancellationToken);

            
            return View(listModel);

        }
        public async Task<IActionResult> New()
        {
            return View();
        }
    }
}
