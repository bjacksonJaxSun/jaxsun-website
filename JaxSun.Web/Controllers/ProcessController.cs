using Microsoft.AspNetCore.Mvc;

namespace JaxSun.Web.Controllers
{
    public class ProcessController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}