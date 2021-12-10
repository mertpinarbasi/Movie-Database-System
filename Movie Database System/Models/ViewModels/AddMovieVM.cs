using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_Database_System.Models.ViewModels
{
    public class AddMovieVM
    {
        public string name { get; set; }
        public string date { get; set; }
        public string genre { get; set; }

        public int rating { get; set; }
        public string trailerURL { get; set; }
        public string summary { get; set; }
        public IFormFile image { get; set; }

        #region implicit 

        public static implicit operator Movie(AddMovieVM movieModel)
        {
            return new Movie
            (

                 movieModel.name,
                  movieModel.date,
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
