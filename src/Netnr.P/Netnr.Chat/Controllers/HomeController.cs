using Microsoft.AspNetCore.Mvc;

namespace Netnr.Chat.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
