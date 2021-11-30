using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Movie_Database_System.Models;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Collections;

namespace Movie_Database_System.Controllers.Movie
{
    public class MovieController : Controller
    {
        private IConfiguration _config;

        public MovieController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult AddMovie()
        {
            ViewData["checkboxFlag"] = " ";
            List<Movie_Database_System.Models.Director> directors = new List<Movie_Database_System.Models.Director>();
            
            try
            {
                var connection = new SqlConnection(_config.GetValue<string>("ConnectionStrings:MovieAppDB").ToString());
                var command = new SqlCommand("getAllDirectors", connection);
                command.CommandType = System.Data.CommandType.StoredProcedure;

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    directors.Add(new Movie_Database_System.Models.Director(reader.GetString(0), reader.GetString(1), reader.GetInt32(2), reader.GetInt32(3)));
                }

                ViewData["DirectorList"] = directors;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("-------------------------");
                Console.WriteLine(e.StackTrace);
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddMovie(Movie_Database_System.Models.ViewModels.AddMovieVM movieVM, Movie_Database_System.Models.ViewModels.AddDirectorVM directorVM)
        {
            #region implicit 
            Movie_Database_System.Models.Movie newMovie = movieVM;
            Movie_Database_System.Models.Director newDirector = directorVM;
            newMovie.movieId = 1;
            newDirector.id = 1;
            #endregion

            Console.WriteLine(movieVM.image.OpenReadStream());
            try
            {
                string filepath = Path.Combine(Startup.hostEnvironment.ContentRootPath + "\\Data\\MovieImages", movieVM.image.FileName);
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

        public IActionResult AddMovieTest(string checkBox)
        {
            Console.WriteLine(checkBox);
            return null;
        }
    }
}
