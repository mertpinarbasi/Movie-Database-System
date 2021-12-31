using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Movie_Database_System.Models.ViewModels;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Text.Json;

namespace Movie_Database_System.Controllers.User
{
    public class UserController : Controller
    {
        /*----------------------------------------------------
            PBKDF2: Password Based Key Derivation Function (2)
            Procedure follows:
            1. Creating a new salt (Salt is a random string that is added to hash)
            2. Call Rfc2898DeriveBytes(string s, stirng salt, int iteration)
                --> the string 's' is hashed 'iteration' times repeatedly, using the 'salt'
            3. Save a pseudo-random key of 20 bytes as a hash
            4. Put the newly created hash and salt together again in a new array
            5. Convert this new hash to Base64 and return
        ----------------------------------------------------*/
        [NonAction]
        public string Hash(string pw)
        {

            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            var pbkdf2 = new Rfc2898DeriveBytes(pw, salt, 100000);
            byte[] hash = pbkdf2.GetBytes(20);

            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            return Convert.ToBase64String(hashBytes);
        }

        [NonAction]
        public bool Verify(string pw, string pwHash)
        {
            /* Extract the bytes */
            byte[] hashBytes = Convert.FromBase64String(pwHash);
            /* Get the salt */
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);
            /* Compute the hash on the password the user entered */
            var pbkdf2 = new Rfc2898DeriveBytes(pw, salt, 100000);
            byte[] hash = pbkdf2.GetBytes(20);
            /* Compare the results */
            for (int i = 0; i < 20; i++)
                if (hashBytes[i + 16] != hash[i])
                    return false;
            return true;
        }
        public IActionResult Register()
        {
            if (HttpContext.Session.GetString("_Username") != null) 
            {
                ViewBag.Privilege = Int32.Parse(JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Privilege")));
            }
            return View();
        }

        [HttpPost]
        public IActionResult Register(UserRegisterVM userRegisterViewModel)
        {
            #region implicit 
            Movie_Database_System.Models.User newUser = userRegisterViewModel;
            newUser.userId = 1;
            #endregion

            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Information given to register was invalid. Plase fill the form correctly.";
                if (HttpContext.Session.GetString("_Username") != null)
                {
                    ViewBag.Privilege = Int32.Parse(JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Privilege")));
                }
                return View();
            }

            try
            {
                var connection = new SqlConnection(Startup.databaseConnStr);
                var command = new SqlCommand("login", connection);

                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add("@username", System.Data.SqlDbType.NVarChar, 20).Value = newUser.username;

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    ViewBag.Error = "Given username is already registered to someone else. Please try to register with another username.";
                    if (HttpContext.Session.GetString("_Username") != null)
                    {
                        ViewBag.Privilege = Int32.Parse(JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Privilege")));
                    }
                    return View();
                }
                else
                {
                    reader.Close();
                    var command2 = new SqlCommand("registerUser", connection);

                    command2.CommandType = System.Data.CommandType.StoredProcedure;

                    command2.Parameters.Add("@name", System.Data.SqlDbType.NVarChar, 20).Value = newUser.name;
                    command2.Parameters.Add("@surname", System.Data.SqlDbType.NVarChar, 30).Value = newUser.surname;
                    command2.Parameters.Add("@username", System.Data.SqlDbType.NVarChar, 20).Value = newUser.username;
                    command2.Parameters.Add("@password", System.Data.SqlDbType.NVarChar, 48).Value = Hash(newUser.password);
                    
                    command2.ExecuteNonQuery();
                    connection.Close();

                    ViewBag.User = newUser.name;
                    if (HttpContext.Session.GetString("_Username") != null)
                    {
                        ViewBag.Privilege = Int32.Parse(JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Privilege")));
                    }
                    return View();
                }
                
            }
            catch (Exception)
            {
                ViewBag.Error = "Something went wrong while registering. Please refresh the page and try again.";
                if (HttpContext.Session.GetString("_Username") != null)
                {
                    ViewBag.Privilege = Int32.Parse(JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Privilege")));
                }
                return View();
            }
        }

        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("_Username") != null)
            {
                ViewBag.Username = JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Username"));
                ViewBag.Privilege = Int32.Parse(JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Privilege")));
            }
            if (TempData["LogoutOk"] != null)
            {
                ViewBag.LoggedOut = true;
                TempData.Remove("LogoutOk");
            }
            return View();
        }

        [HttpPost]
        public IActionResult Login(UserLoginVM userLoginViewModel) 
        {
            #region implicit 
            Movie_Database_System.Models.User loggedUser = userLoginViewModel;
            #endregion

            if(!ModelState.IsValid)
            {
                ViewBag.Error = "The username or password was invalid, or this user is not registered to Movie App Database.";
                if (HttpContext.Session.GetString("_Username") != null)
                {
                    ViewBag.Privilege = Int32.Parse(JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Privilege")));
                }
                return View();
            }

            try
            {
                var connection = new SqlConnection(Startup.databaseConnStr);
                var command = new SqlCommand("login", connection);

                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add("@username", System.Data.SqlDbType.NVarChar, 20).Value = loggedUser.username;

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    Models.User user = new Models.User(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetInt32(5));
                    if (Verify(loggedUser.password, user.password))
                    {
                        reader.Close();
                        connection.Close();

                        // Login Successfull
                        HttpContext.Session.SetString("_Username", JsonSerializer.Serialize(user.username));
                        HttpContext.Session.SetString("_Privilege", JsonSerializer.Serialize(user.privilege.ToString()));
                        
                        TempData["Name"] = user.name;
                        TempData["Privilege"] = user.privilege;

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ViewBag.Error = "The username or password was invalid, or this user is not registered to Movie App Database.";
                        if (HttpContext.Session.GetString("_Username") != null)
                        {
                            ViewBag.Privilege = Int32.Parse(JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Privilege")));
                        }
                        return View();
                    }
                }
                else
                {
                    ViewBag.Error = "The username or password was invalid, or this user is not registered to Movie App Database.";
                    if (HttpContext.Session.GetString("_Username") != null)
                    {
                        ViewBag.Privilege = Int32.Parse(JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Privilege")));
                    }
                    return View();
                }
            }
            catch (Exception)
            {
                ViewBag.Error = "The username or password was invalid, or this user is not registered to Movie App Database.";
                if (HttpContext.Session.GetString("_Username") != null)
                {
                    ViewBag.Privilege = Int32.Parse(JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Privilege")));
                }
                return View();
            }
        }

        public IActionResult Logout()
        {
            if (HttpContext.Session.GetString("_Username") != null)
            {
                TempData["LogoutOk"] = JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Username"));
                HttpContext.Session.Remove("_Username");
                HttpContext.Session.Remove("_Privilege");
                return RedirectToAction("Login", "User");
            }
            else
            {
                return RedirectToAction("Login", "User");
            }
        }

        public IActionResult Privileges()
        {
            if (HttpContext.Session.GetString("_Username") != null)
            {
                if (Int32.Parse(JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Privilege"))) > 1)
                {
                    List<Models.User> normalUsers = new List<Models.User>();

                    var connection = new SqlConnection(Startup.databaseConnStr);
                    var command = new SqlCommand("getUsers", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    try
                    {
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            normalUsers.Add(new Models.User(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), " ", reader.GetInt32(4)));
                        }
                        reader.Close();
                        connection.Close();

                        if (TempData["Status"] != null)
                        {
                            ViewBag.Previous = TempData["Status"];
                            TempData.Remove("Status");
                        }
                        ViewData["NormalUsers"] = normalUsers;
                        ViewBag.Privilege = Int32.Parse(JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Privilege")));
                        ViewBag.LoggedUser = JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Username"));
                        return View();
                    }
                    catch (Exception)
                    {
                        ViewBag.Error = "Something went wrong while retrieving users. Please try to refresh page or try again a little while later.";
                        ViewBag.Privilege = Int32.Parse(JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Privilege")));
                        return View();
                    }
                }
                else
                {
                    ViewBag.Error = "You don't have authorization required to elevate a user's privileges.";
                    ViewBag.Privilege = Int32.Parse(JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Privilege")));
                    return View();
                }
            }
            else
            {
                ViewBag.Error = "You don't have authorization required to elevate a user's privileges.";
                return View();
            }
        }

        public IActionResult SetAsAdmin(string id)
        {
            if (HttpContext.Session.GetString("_Username") != null)
            {
                if (Int32.Parse(JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Privilege"))) > 1)
                {
                    try
                    {
                        var connection = new SqlConnection(Startup.databaseConnStr);
                        var command = new SqlCommand("SetUserAsAdmin", connection);

                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add("@userId", System.Data.SqlDbType.Int).Value = Int32.Parse(id);

                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();

                        TempData["Status"] = true;
                        return RedirectToAction("Privileges", "User");
                    }
                    catch (Exception)
                    {
                        TempData["Status"] = false;
                        return RedirectToAction("Privileges", "User");
                    }
                }
                else
                {
                    return RedirectToAction("Login", "User");
                }
            }
            else
            {
                return RedirectToAction("Login", "User");
            }
        }

        public IActionResult SetAsNormal(string id)
        {
            if (HttpContext.Session.GetString("_Username") != null)
            {
                if (Int32.Parse(JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Privilege"))) > 1)
                {
                    try
                    {
                        var connection = new SqlConnection(Startup.databaseConnStr);
                        var command = new SqlCommand("SetUserAsNormal", connection);

                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add("@userId", System.Data.SqlDbType.Int).Value = Int32.Parse(id);

                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();

                        TempData["Status"] = true;
                        return RedirectToAction("Privileges", "User");
                    }
                    catch (Exception)
                    {
                        TempData["Status"] = false;
                        return RedirectToAction("Privileges", "User");
                    }
                }
                else
                {
                    return RedirectToAction("Login", "User");
                }
            }
            else
            {
                return RedirectToAction("Login", "User");
            }
        }
    }
}
