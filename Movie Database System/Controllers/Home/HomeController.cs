using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Movie_Database_System.Models;
using System;
using System.Collections.Generic;
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
            // Azure Database Connection Test
            try 
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "movieappserver.database.windows.net";
                builder.UserID = "turkay7879";
                builder.Password = "s4msep!0l";
                builder.InitialCatalog = "MovieAppDatabase";

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    // Create person table
                    String sql = "SELECT * FROM [dbo].[Person]";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // reader.GetX() fonksiyonları, gelen row'un columnlarına karşılık geliyor
                                // Person tablosu için reader.GetInt32(0)  --> Id
                                //                     reader.GetString(1) --> Name vs.
                                Console.WriteLine("Name: {0}, Surname: {1}", reader.GetString(1), reader.GetString(2));
                            }
                        }
                    }
                }
            }
            catch (Exception e) 
            {
                Console.WriteLine("An error occured with database querying");
                Console.WriteLine(e);
            }
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
