using Microsoft.AspNetCore.Mvc;

namespace Movie_Database_System.Controllers.Actor
{
    public class ActorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
