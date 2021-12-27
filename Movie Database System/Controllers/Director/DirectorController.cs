using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Movie_Database_System.Controllers.Director
{
    public class DirectorController : Controller
    {

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
                ViewBag.Error = err.Message;
                return View();
            }

            ViewData["Director"] = director;
            ViewData["MovieList"] = moviesOfDirector;
            if (HttpContext.Session.GetString("_Username") != null)
            {
                ViewBag.Privilege = Int32.Parse(JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Privilege")));
            }
            return View();
        }

        public IActionResult DeleteDirector(string id)
        {
            if (HttpContext.Session.GetString("_Username") != null)
            {
                if (Int32.Parse(JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Privilege"))) > 0)
                {
                    var connection = new SqlConnection(Startup.databaseConnStr);
                    try
                    {
                        var command = new SqlCommand("deleteDirector", connection);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add("@DirectorId", System.Data.SqlDbType.Int).Value = Int32.Parse(id);

                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                        ViewBag.Message = "Silme işlemi başarılı !";
                        ViewBag.Privilege = Int32.Parse(JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Privilege")));
                        return View();
                    }

                    catch (Exception err)
                    {
                        ViewBag.Error = err.Message;
                        ViewBag.Privilege = Int32.Parse(JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Privilege")));
                        return View();
                    }
                }
                else
                {
                    ViewBag.Error = "You don't have authorization required to delete a director and it's connected data.";
                    return View();
                }
            }
            else
            {
                ViewBag.Error = "You don't have authorization required to delete a director and it's connected data.";
                return View();
            }
        }
    }
}
