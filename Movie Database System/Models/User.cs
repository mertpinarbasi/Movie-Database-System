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
        public int privilege { get; set; }

        public User(int userId, string name, string surname, string username, string password, int privilege)
        {
            this.userId = userId;
            this.name = name;
            this.surname = surname;
            this.username = username;
            this.password = password;
            this.privilege = privilege;
        }

        #region implicit 
        public static implicit operator User(UserRegisterVM userModel)
        {
            return new User(
            0,
            userModel.name,
            userModel.surname,
            userModel.username,
            userModel.password,
            0
            );
        }
        public static implicit operator User(UserLoginVM userModel)
        {
            return new User(
                0,
                " ",
                " ",
                userModel.username,
                userModel.password,
                0
            );
        }
        #endregion
    }
}

