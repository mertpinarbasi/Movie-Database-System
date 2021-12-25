using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Movie_Database_System.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_Database_System.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            List<Models.Movie> topThreeMovies = new List<Models.Movie>();
            List<Models.Movie> latestThreeMovies = new List<Models.Movie>();

            var connection = new SqlConnection(Startup.databaseConnStr);
            try
            {
                /* Retrieve top 3 movies from DB */
                var command = new SqlCommand("getTopThreeMovies", connection);
                command.CommandType = System.Data.CommandType.StoredProcedure;

                connection.Open();
                SqlDataReader movieReader = command.ExecuteReader();
                while (movieReader.Read())
                {
                    topThreeMovies.Add(new Models.Movie(movieReader.GetString(0), movieReader.GetString(1), movieReader.GetString(2), (byte[])movieReader[3]));
                }
                movieReader.Close();

                /* Retrieve latest added 3 movies from DB */
                var command2 = new SqlCommand("getLatestThreeMovies", connection);
                command2.CommandType = System.Data.CommandType.StoredProcedure;

                SqlDataReader movieReader2 = command2.ExecuteReader();
                while (movieReader2.Read())
                {
                    latestThreeMovies.Add(new Models.Movie(movieReader2.GetString(0), movieReader2.GetString(1), movieReader2.GetString(2), (byte[])movieReader2[3]));
                }
                movieReader2.Close();

                connection.Close();
            }
            catch (Exception err)
            {
                return Json(err.ToString());
            }

            ViewData["TopThreeList"] = topThreeMovies;
            ViewData["LatestThreeList"] = latestThreeMovies;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
