using Microsoft.AspNetCore.Mvc;

namespace Movie_Database_System.Controllers.Director
{
    public class DirectorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddDirector()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddDirector(Movie_Database_System.Models.ViewModels.AddDirectorVM directorVM)
        {
            #region implicit
            Movie_Database_System.Models.Director director = directorVM;
            #endregion


            return View();
        }
    }
}
