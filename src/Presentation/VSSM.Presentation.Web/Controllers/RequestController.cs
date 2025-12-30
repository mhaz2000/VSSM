using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VSSM.Presentation.Web.Controllers
{
    [Authorize]
    public class RequestController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
