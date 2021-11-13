using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Movie_Database_System.Models.ViewModels;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_Database_System.Controllers.User
{
    public class UserController : Controller
    {
        private IConfiguration _config;

        public UserController(IConfiguration config)
        {
            _config = config;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RegisterUser( UserRegisterVM userRegisterViewModel)
        {
            #region implicit 
            Movie_Database_System.Models.User newUser = userRegisterViewModel;
            newUser.userId = 1;
            #endregion

            if (!ModelState.IsValid)
            {
                return Json(ModelState.Values.FirstOrDefault().Errors);
            }

            string sql = "INSERT INTO [dbo].[User] ([Name], [Surname], [Username], [Password])"
                + "\tVALUES (@n, @s, @u, @p)";
            using (var connection = new SqlConnection(_config.GetValue<string>("ConnectionStrings:MovieAppDB").ToString()))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@n", System.Data.SqlDbType.NVarChar, 20).Value = newUser.name;
                command.Parameters.Add("@s", System.Data.SqlDbType.NVarChar, 30).Value = newUser.surname;
                command.Parameters.Add("@u", System.Data.SqlDbType.NVarChar, 20).Value = newUser.username;
                command.Parameters.Add("@p", System.Data.SqlDbType.NVarChar, 20).Value = newUser.password;

                connection.Open();
                command.ExecuteNonQuery();
            }

            return Json(newUser);
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult LoginUser(UserLoginVM userLoginViewModel ) 
        {


            #region implicit 
            Movie_Database_System.Models.User loggedUser = userLoginViewModel;
           
            #endregion


            if(!ModelState.IsValid)
            {
                return Json(ModelState.Values.FirstOrDefault().Errors); 
            }
            return Json(loggedUser);
           
        }




    }
}
