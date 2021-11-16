﻿using Microsoft.AspNetCore.Http;
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
        public string date { get; set; }
        public string genre { get; set; }

        public int rating { get; set; }
        public string trailerURL { get; set; }
        public string summary { get; set; }
        public IFormFile image { get; set; }
    }
}