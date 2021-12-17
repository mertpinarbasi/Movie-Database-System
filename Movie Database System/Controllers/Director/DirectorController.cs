using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

        public IActionResult GetDirector(string id)
        {
            Models.Director director = new Models.Director();
            List<Models.Movie> moviesOfDirector = new List<Models.Movie>();

            var connection = new SqlConnection(Startup.databaseConnStr);
            try
            {
                var command = new SqlCommand("getDirector", connection);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add("@id", System.Data.SqlDbType.Int).Value = Int32.Parse(id);

                connection.Open();
                SqlDataReader dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    director.name = dataReader.GetString(0);
                    director.surname = dataReader.GetString(1);
                    director.age = dataReader.GetInt32(2);

                    Models.Movie movie = new Models.Movie(dataReader.GetInt32(3), dataReader.GetString(4), dataReader.GetDateTime(5), dataReader.GetString(6), (int)dataReader.GetDouble(7));
                    movie.summary = dataReader.GetString(8);
                    moviesOfDirector.Add(movie);
                }
                dataReader.Close();
                connection.Close();
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
                return Json(err.ToString());
            }

            ViewData["Director"] = director;
            ViewData["MovieList"] = moviesOfDirector;
            return View();
        }
    }
}
