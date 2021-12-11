using System.ComponentModel.DataAnnotations;

namespace Movie_Database_System.Models.ViewModels
{
    public class AddActorVM
    {
        [Required(ErrorMessage = "Please enter a name for actor.")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Name value must have a length between 2 and 15.")]
        public string actorName { get; set; }

        [Required(ErrorMessage = "Please enter a surname for actor.")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Surname value must have a length between 2 and 15.")]
        public string actorSurname { get; set; }

        [Required(ErrorMessage = "Please enter an age for actor.")]
        [Range(18, 110, ErrorMessage = "Age value must be between 18 and 110.")]
        public int actorAge { get; set; }

        #region implicit
        public static implicit operator Actor(AddActorVM actorVM)
        {
            return new Actor
            (
                actorVM.actorName,
                actorVM.actorSurname,
                actorVM.actorAge
            );
        }
        #endregion
    }
}
