using Microsoft.AspNetCore.Mvc;

namespace EventBooking.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
