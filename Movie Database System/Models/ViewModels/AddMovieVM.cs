using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_Database_System.Models.ViewModels
{
    public class AddMovieVM
    {
        [Required(ErrorMessage = "Please enter a movie name")]
        [StringLength(75, MinimumLength = 2, ErrorMessage = "Name value must have a length between 2 and 75.")]
        public string name { get; set; }
        public string date { get; set; }

        [Required(ErrorMessage = "Please enter a genre")]
        [StringLength(15, MinimumLength = 4, ErrorMessage = "Genre value must have a length between 4 and 15.")]
        public string genre { get; set; }

        [Required(ErrorMessage = "Please give a rating for movie.")]
        [Range(0, 10, ErrorMessage = "Rating value must be between 0 and 10.")]
        public int rating { get; set; }

        [StringLength(50, MinimumLength = 10, ErrorMessage = "Trailer URL must have a length between 10 and 50.")]
        public string trailerURL { get; set; }

        [Required(ErrorMessage = "Please enter movie summary")]
        public string summary { get; set; }
        public IFormFile image { get; set; }

        #region implicit 

        public static implicit operator Movie(AddMovieVM movieModel)
        {
            return new Movie
            (
                movieModel.name,
                DateTime.Parse(movieModel.date),
                movieModel.genre,
                movieModel.rating,
                movieModel.trailerURL,
                movieModel.summary,
                movieModel.image
            );
        }

        #endregion

    }
}
