using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

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
                ViewData["Error"] = err;
                return View();
            }

            ViewData["MovieList"] = movieInfo;
            return View();
        }

        [HttpPost]
        public IActionResult AddActor(Movie_Database_System.Models.ViewModels.AddActorVM actorVM, string chosenMovie)
        {
            Movie_Database_System.Models.Actor newActor = actorVM;

            if (ModelState.IsValid)
            {
                int chosenMovieId = Int32.Parse(chosenMovie);

                var connection = new SqlConnection(Startup.databaseConnStr);
                int newActorId = -1;
                try
                {
                    /* Get a list of actors to see if new actor already exists */
                    List<Models.Actor> allActors = new List<Models.Actor>();
                    var command = new SqlCommand("getAllActors", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    connection.Open();
                    SqlDataReader actorReader = command.ExecuteReader();
                    while (actorReader.Read())
                    {
                        allActors.Add(new Models.Actor(actorReader.GetString(1), actorReader.GetString(2), actorReader.GetInt32(3), actorReader.GetInt32(0)));
                    }
                    actorReader.Close();

                    bool alreadyExists = false;
                    foreach (Models.Actor actor in allActors)
                    {
                        if (actor.name == newActor.name && actor.surname == newActor.surname && actor.age == newActor.age)
                        {
                            alreadyExists = true;
                            newActorId = actor.id;
                            break;
                        }
                    }

                    if (!alreadyExists)
                    {
                        /* Upload new actor to database */
                        var command2 = new SqlCommand("addActor", connection);

                        command2.CommandType = System.Data.CommandType.StoredProcedure;
                        command2.Parameters.Add("@name", System.Data.SqlDbType.NVarChar, 20).Value = newActor.name;
                        command2.Parameters.Add("@surname", System.Data.SqlDbType.NVarChar, 20).Value = newActor.surname;
                        command2.Parameters.Add("@age", System.Data.SqlDbType.Int).Value = newActor.age;
                        command2.Parameters.Add("@newActorId", System.Data.SqlDbType.Int);
                        command2.Parameters["@newActorId"].Direction = System.Data.ParameterDirection.Output;

                        command2.ExecuteNonQuery();
                        newActorId = (int)command2.Parameters["@newActorId"].Value;
                        newActor.id = newActorId;
                    }

                    /* Update played movie table for the new actor */
                    var command3 = new SqlCommand("updatePlayedMovies", connection);

                    command3.CommandType = System.Data.CommandType.StoredProcedure;
                    command3.Parameters.Add("@actorid", System.Data.SqlDbType.Int).Value = newActorId;
                    command3.Parameters.Add("@movieid", System.Data.SqlDbType.Int).Value = chosenMovieId;

                    command3.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception err)
                {
                    ViewData["Error"] = err;
                    return View();
                }

                return Json(newActor);
            }

            return Json(ModelState.Values.FirstOrDefault().Errors);
        }

        public IActionResult GetActor(string id)
        {
            Models.Actor actor = new Models.Actor();
            List<String> moviesOfActor = new List<String>();
            String movieName;
            var connection = new SqlConnection(Startup.databaseConnStr);
            
            try
            {
                var command = new SqlCommand("getActor", connection);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add("@ActorID", System.Data.SqlDbType.Int).Value = Int32.Parse(id);

                connection.Open();
                SqlDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    actor.name = dataReader.GetString(0);
                    actor.surname = dataReader.GetString(1);
                    actor.age = dataReader.GetInt32(2);

                    movieName = dataReader.GetString(3);
                    moviesOfActor.Add(movieName);
                }

                dataReader.Close();
                connection.Close();

            }
            catch (Exception err)
            {
                ViewBag.error = err;
                return View();
            }

            ViewData["Actor"] = actor;
            ViewData["Movies"] = moviesOfActor;

            return View();
        }


    }
}
