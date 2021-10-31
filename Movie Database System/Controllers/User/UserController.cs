using Microsoft.AspNetCore.Mvc;
using Movie_Database_System.Models.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_Database_System.Controllers.User
{
    public class UserController : Controller
    {
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
            newUser.userId = -1; 
            #endregion
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
            return Json(loggedUser);
           
        }




    }
}
