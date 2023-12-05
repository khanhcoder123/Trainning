using Microsoft.AspNetCore.Mvc;

namespace Tranning.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
