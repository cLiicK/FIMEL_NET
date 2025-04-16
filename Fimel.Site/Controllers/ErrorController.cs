using Microsoft.AspNetCore.Mvc;

namespace Fimel.Site.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Error()
        {
            return View("ErrorGenerico");
        }
    }
}
