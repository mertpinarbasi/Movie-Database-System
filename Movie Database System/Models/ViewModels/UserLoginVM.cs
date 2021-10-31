using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_Database_System.Models.ViewModels
{
    public class UserLoginVM
    {


        [Required(ErrorMessage = "Username value can not be empty ")]
        [StringLength(5, ErrorMessage = "Invalid username")]
        public string username { get; set; }
        [Required(ErrorMessage = "Please enter your surname")]
        [StringLength(5, ErrorMessage = "Invalid password" )]
        public string password { get; set; }
    }
}
