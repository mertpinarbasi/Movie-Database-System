using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Movie_Database_System.Models;
using System.IO;

namespace Movie_Database_System.Controllers.Movie
{
    public class MovieController : Controller
    {
        public IActionResult AddMovie()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddMovie(Movie_Database_System.Models.ViewModels.AddMovieVM movieVM)
        {
            #region implicit 
            Movie_Database_System.Models.Movie newMovie = movieVM;
            newMovie.movieId = 1;
            #endregion

            Console.WriteLine(movieVM.image.OpenReadStream());
            try
            {
                string filepath = Path.Combine("../../", movieVM.image.FileName);

                using (Stream filestream = new FileStream(filepath, FileMode.Create))
                {
                    await movieVM.image.CopyToAsync(filestream);
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
            }


            return Json(newMovie);
        }


    }
}
