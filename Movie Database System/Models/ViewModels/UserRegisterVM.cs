using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_Database_System.Models.ViewModels
{
    public class UserRegisterVM
    {
        [Required(ErrorMessage ="Please enter your name")] 
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Name value must have a length between 2 and 20.")] // ErrorMessage ="Name value must be no longer than 20 characters"
        public string name { get; set; }

        [Required(ErrorMessage ="Please enter your surname")] 
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Surname value must have a length between 2 and 30.")] // , ErrorMessage = "Surname value must be no longer than 30 characters "
        public string surname { get; set; }

        [Required(ErrorMessage = "Please enter a username")]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "Username value must have a length between 5 and 20.")] // , ErrorMessage = "Surname value must be no longer than 20 characters "
        public string username { get; set; }

        [Required(ErrorMessage = "Please enter a password")]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "Password value must have a length between 5 and 20.")] // , ErrorMessage = "Surname value must be no greater than 20 characters "
        public string password { get; set; }
    }
}
