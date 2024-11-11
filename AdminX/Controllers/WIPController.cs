using Microsoft.AspNetCore.Mvc;

namespace AdminX.Controllers
{
    public class WIPController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult NotFound()
        {
            return View();
        }
    }
}
