using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Movie_Database_System.Controllers.Actor
{
    public class ActorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddActor()
        {
            List<Models.Movie> movieInfo = new List<Models.Movie>();

            var connection = new SqlConnection(Startup.databaseConnStr);
            try
            {
                var command = new SqlCommand("getAllMovieInfo", connection);
                command.CommandType = System.Data.CommandType.StoredProcedure;

                connection.Open();
                SqlDataReader movieReader = command.ExecuteReader();
                while(movieReader.Read())
                {
                    movieInfo.Add(new Models.Movie(movieReader.GetInt32(0), movieReader.GetString(1), movieReader.GetDateTime(2), movieReader.GetString(3), Convert.ToInt32(movieReader.GetDouble(4))));
                }
                movieReader.Close();
                connection.Close(); 
            }
            catch (Exception err)
            {
                return Json(err.ToString());
            }

            ViewData["MovieList"] = movieInfo;
            return View();
        }

        [HttpPost]
        public IActionResult AddActor(Movie_Database_System.Models.ViewModels.AddActorVM actorVM, string chosenMovie)
        {
            Movie_Database_System.Models.Actor newActor = actorVM;
            int chosenMovieId = Int32.Parse(chosenMovie);

            var connection = new SqlConnection(Startup.databaseConnStr);
            int newActorId = -1;
            try
            {
                /* Upload new actor to database */
                var command = new SqlCommand("addActor", connection);

                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add("@name", System.Data.SqlDbType.NVarChar, 20).Value = newActor.name;
                command.Parameters.Add("@surname", System.Data.SqlDbType.NVarChar, 20).Value = newActor.surname;
                command.Parameters.Add("@age", System.Data.SqlDbType.Int).Value = newActor.age;
                command.Parameters.Add("@newActorId", System.Data.SqlDbType.Int);
                command.Parameters["@newActorId"].Direction = System.Data.ParameterDirection.Output;

                connection.Open();
                command.ExecuteNonQuery();
                newActorId = (int)command.Parameters["@newActorId"].Value;
                newActor.id = newActorId;

                /* Update played movie table for the new actor */
                var command2 = new SqlCommand("updatePlayedMovies", connection);

                command2.CommandType = System.Data.CommandType.StoredProcedure;
                command2.Parameters.Add("@actorid", System.Data.SqlDbType.Int).Value = newActorId;
                command2.Parameters.Add("@movieid", System.Data.SqlDbType.Int).Value = chosenMovieId;

                command2.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception err)
            {
                return Json(err.ToString());
            }
            
            return Json(newActor);
        }
    }
}
