using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_Database_System.Models.ViewModels
{
    public class UserRegisterVM
    {
        [Required] [StringLength(3)]
        public string name { get; set; }
        [Required] [StringLength(3)]
        public string surname { get; set; }
        [Required][StringLength(8)]
        public string username { get; set; }
        [Required][StringLength(5)]
        public string password { get; set; }
    }
}
