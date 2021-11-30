using System.ComponentModel.DataAnnotations;

namespace Movie_Database_System.Models.ViewModels
{
    public class AddDirectorVM
    {
        [Required(ErrorMessage = "Please enter a name for director.")]
        [StringLength(15, MinimumLength = 2, ErrorMessage = "Name value must have a length between 2 and 15.")]
        public string directorName { get; set; }

        [Required(ErrorMessage = "Please enter a surname for director.")]
        [StringLength(15, MinimumLength = 2, ErrorMessage = "Surname value must have a length between 2 and 15.")]
        public string directorSurname { get; set; }

        [Required(ErrorMessage = "Please enter an age for director.")]
        [Range(18, 110, ErrorMessage = "Age value must be between 18 and 110.")]
        public int directorAge { get; set; }

        #region implicit
        public static implicit operator Director(AddDirectorVM directorVM)
        {
            return new Director
            (
                directorVM.directorName,
                directorVM.directorSurname,
                directorVM.directorAge
            );
        }
        #endregion
    }
}
