using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_Database_System.Models.ViewModels
{
    public class UserLoginVM
    {
        [Required(ErrorMessage = "Please enter a username")]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "Username value must have a length between 5 and 20.")]
        public string username { get; set; }
        [Required(ErrorMessage = "Please enter a password")]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "Password value must have a length between 5 and 20.")]
        public string password { get; set; }
    }
}
