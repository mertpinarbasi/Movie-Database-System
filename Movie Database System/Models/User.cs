using Movie_Database_System.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_Database_System.Models
{
    public class User
    {


        public int userId { get; set; }
        public string name { get; set; }
        public string surname { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        #region implicit 
        public static implicit operator User(UserRegisterVM userModel)
        {
            return new User
            {
                name = userModel.name,
            surname = userModel.surname,
            username = userModel.username,
            password = userModel.password

        };
    }


        public static implicit operator User(UserLoginVM userModel)
        {
            return new User
            {
          
                username = userModel.username,
                password = userModel.password

            };
        }







        #endregion

    }
}
