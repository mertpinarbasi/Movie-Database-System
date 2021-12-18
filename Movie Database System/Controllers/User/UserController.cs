using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Movie_Database_System.Models.ViewModels;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Movie_Database_System.Controllers.User
{
    public class UserController : Controller
    {
        private IConfiguration _config;
        private IWebHostEnvironment _hostEnvironment;

        public UserController(IConfiguration config, IWebHostEnvironment hostEnvironment)
        {
            _config = config;
            _hostEnvironment = hostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }

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
                    throw new Exception("This username is already taken. Please register with a different username.");
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
                }
                connection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("-------------------------");
                Console.WriteLine(e.StackTrace);
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
                    string dbUsername = reader.GetString(0);
                    string dbPassword = reader.GetString(1);
                    if (Verify(loggedUser.password, dbPassword))
                    {
                        Console.WriteLine("Login Successfull!");
                    }
                    else
                    {
                        throw new Exception("The username or password is invalid. Login failed.");
                    }
                }
                else
                {
                    throw new Exception("The username or password is invalid. Login failed.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("-------------------------");
                Console.WriteLine(e.StackTrace);
            }

            return Json(loggedUser);
        }
    }
}
