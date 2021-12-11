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
            List<string> movieInfo = new List<string>();


            return View();
        }

        [HttpPost]
        public IActionResult AddActor(Movie_Database_System.Models.ViewModels.AddActorVM actorVM)
        {
            Movie_Database_System.Models.Actor newActor = actorVM;

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
            }
            catch (Exception err)
            {
                return Json(err.ToString());
            }
            
            return Json(newActor);
        }
    }
}
