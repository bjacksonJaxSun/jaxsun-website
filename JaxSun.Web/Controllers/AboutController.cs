using Microsoft.AspNetCore.Mvc;

namespace JaxSun.Web.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}