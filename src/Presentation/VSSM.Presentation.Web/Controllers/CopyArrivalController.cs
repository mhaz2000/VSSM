using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VSSM.Presentation.Web.Controllers
{
    
    public class CopyArrivalController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
