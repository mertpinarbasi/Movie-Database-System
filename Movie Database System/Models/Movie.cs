using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_Database_System.Models
{
    public class Movie
    {

        public int movieId { get; set; }
        public string name { get; set; }
        public DateTime date { get; set; }
        public string genre { get; set; }

        public int rating { get; set; }
        public string trailerURL { get; set; }
        public string summary { get; set; }
        public IFormFile image { get; set; }
        public byte[] imageBinary { get; set; }

        public int directorId { get; set; }

        public Movie(int movieId, string name, DateTime date, string genre, int rating)
        {
            this.movieId = movieId;
            this.date = date;
            this.name = name;
            this.genre = genre;
            this.rating = rating;
        }

        public Movie(int movieId, string name, string genre, string summary, byte[] imageBinary)
        {
            this.movieId = movieId;
            this.name = name;
            this.genre = genre;
            this.summary = summary;
            this.imageBinary = imageBinary;
        }
        public Movie(int movieId, string name, DateTime date, string genre, int rating, string trailerURL, string summary, byte[] imageBinary, int directorId)
        {
            this.movieId = movieId;
            this.name = name;
            this.date = date;
            this.genre = genre;
            this.rating = rating;
            this.trailerURL = trailerURL;
            this.summary = summary;
            this.imageBinary = imageBinary;
            this.directorId = directorId;
        }

        public Movie(string name, DateTime date, string genre, int rating, string trailerURL, string summary, IFormFile image)
        {
            this.movieId = movieId;
            this.name = name;
            this.date = date;
            this.genre = genre;
            this.rating = rating;
            this.trailerURL = trailerURL;
            this.summary = summary;
            this.image = image;
        }
    }

}
