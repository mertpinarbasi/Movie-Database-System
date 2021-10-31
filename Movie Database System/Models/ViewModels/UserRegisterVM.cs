using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_Database_System.Models.ViewModels
{
    public class UserRegisterVM
    {
        [Required(ErrorMessage ="Please enter your name")] [StringLength(3,ErrorMessage ="Name value must be longer than 3 characters")]
        public string name { get; set; }
        [Required(ErrorMessage ="Please enter your surname")] [StringLength(3,ErrorMessage ="Surname value must be longer than 3 characters ")]
        public string surname { get; set; }
        [Required(ErrorMessage = "Please enter a username")]
        [StringLength(5, ErrorMessage = "Surname value must be longer than 5 characters ")]
        public string username { get; set; }
        [Required(ErrorMessage = "Please enter a password")]
        [StringLength(5, ErrorMessage = "Surname value must be greater than 5 characters ")]
        public string password { get; set; }
    }
}
