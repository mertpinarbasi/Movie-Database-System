using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Movie_Database_System.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Movie_Database_System.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMemoryCache _memoryCache;
        public HomeController(ILogger<HomeController> logger, IMemoryCache memoryCache)
        {
            _logger = logger;
            _memoryCache = memoryCache;
        }

        
        public IActionResult Index()
        {
            string indexCache1 = "topThreeCache";
            string indexCache2 = "latestThreeCache";
            if (!_memoryCache.TryGetValue(indexCache1, out List<Models.Movie> _))
            {
                List<Models.Movie> updateTopThreeMovies = new List<Models.Movie>();
                var connection = new SqlConnection(Startup.databaseConnStr);
                var command = new SqlCommand("getTopThreeMovies", connection);
                command.CommandType = System.Data.CommandType.StoredProcedure;

                try
                {
                    connection.Open();
                    SqlDataReader movieReader = command.ExecuteReader();
                    while (movieReader.Read())
                    {
                        updateTopThreeMovies.Add(new Models.Movie(movieReader.GetInt32(0), movieReader.GetString(1), movieReader.GetString(2), movieReader.GetString(3), (byte[])movieReader[4]));
                    }
                    movieReader.Close();
                    connection.Close();

                    ViewData["TopThreeList"] = updateTopThreeMovies;

                    var cacheExpiryOptions = new MemoryCacheEntryOptions
                    {
                        Priority = CacheItemPriority.High,
                        SlidingExpiration = TimeSpan.FromSeconds(300), // 5 mins
                        AbsoluteExpiration = DateTime.Now.AddSeconds(60 * 60 * 24 * 3) // 7 days
                    };

                    _memoryCache.Set(indexCache1, updateTopThreeMovies, cacheExpiryOptions);
                }
                catch (Exception)
                {
                    ViewBag.Error = "Something went wrong with getting top three movies. Please try to refresh the page.";
                    return View();
                }
            }
            else
            {
                ViewData["TopThreeList"] = _memoryCache.Get<List<Models.Movie>>(indexCache1);
            }

            if (!_memoryCache.TryGetValue(indexCache2, out List<Models.Movie> _))
            {
                List<Models.Movie> updateLatestThreeMovies = new List<Models.Movie>();
                var connection = new SqlConnection(Startup.databaseConnStr);
                var command2 = new SqlCommand("getLatestThreeMovies", connection);
                command2.CommandType = System.Data.CommandType.StoredProcedure;

                try
                {
                    connection.Open();
                    SqlDataReader movieReader2 = command2.ExecuteReader();
                    while (movieReader2.Read())
                    {
                        updateLatestThreeMovies.Add(new Models.Movie(movieReader2.GetInt32(0), movieReader2.GetString(1), movieReader2.GetString(2), movieReader2.GetString(3), (byte[])movieReader2[4]));
                    }
                    movieReader2.Close();
                    connection.Close();

                    ViewData["LatestThreeList"] = updateLatestThreeMovies;

                    var cacheExpiryOptions = new MemoryCacheEntryOptions
                    {
                        Priority = CacheItemPriority.High,
                        SlidingExpiration = TimeSpan.FromSeconds(300), // 5 mins
                        AbsoluteExpiration = DateTime.Now.AddSeconds(60 * 60 * 24 * 3) // 7 days
                    };

                    _memoryCache.Set(indexCache2, updateLatestThreeMovies, cacheExpiryOptions);
                }
                catch (Exception)
                {
                    ViewBag.Error = "Something went wrong with getting latest three movies. Please try to refresh the page.";
                    return View();
                }
            }
            else
            {
                ViewData["LatestThreeList"] = _memoryCache.Get<List<Models.Movie>>(indexCache2); 
            }

            if (TempData["Name"] != null)
            {
                ViewData["Name"] = TempData["Name"].ToString();
                TempData.Remove("Name");
            }
            if (HttpContext.Session.GetString("_Username") != null)
            {
                ViewBag.Privilege = Int32.Parse(JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Privilege")));
            }
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
