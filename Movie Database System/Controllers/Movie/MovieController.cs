using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Movie_Database_System.Models;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Collections;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Movie_Database_System.Controllers.Movie
{
    public class MovieController : Controller
    {
        private List<Movie_Database_System.Models.Director> directors;

        private string Separate(string filePath)
        {
            string output = "";
            string[] folders = filePath.Split(';');

            output += Path.DirectorySeparatorChar.ToString();
            output += folders[0];
            output += Path.DirectorySeparatorChar.ToString();
            output += folders[1];
            output += Path.DirectorySeparatorChar.ToString();

            return output;
        }

        public MovieController(IConfiguration config)
        {
            directors = new List<Movie_Database_System.Models.Director>();
        }

        public IActionResult AddMovie()
        {
            try
            {
                var connection = new SqlConnection(Startup.databaseConnStr);
                var command = new SqlCommand("getAllDirectors", connection);
                command.CommandType = System.Data.CommandType.StoredProcedure;

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    directors.Add(new Movie_Database_System.Models.Director(reader.GetString(0), reader.GetString(1), reader.GetInt32(2), reader.GetInt32(3)));
                }

                ViewData["DirectorList"] = directors;
                reader.Close();
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message;
                return View();
            }

            if (HttpContext.Session.GetString("_Username") != null)
            {
                ViewBag.Privilege = Int32.Parse(JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Privilege")));
            }
            return View();
        }
        public IActionResult SearchMovie()
        {
            if (HttpContext.Session.GetString("_Username") != null)
            {
                ViewBag.Privilege = Int32.Parse(JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Privilege")));
            }
            return View();
        }
        [HttpPost]
        public IActionResult SearchMovie(string movieName)
        {
            var connection = new SqlConnection(Startup.databaseConnStr);
            var command = new SqlCommand("Search_Movie", connection);
            List<Movie_Database_System.Models.Movie> movies = new List<Models.Movie>();

            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.Parameters.Add("@searchInput", System.Data.SqlDbType.NVarChar).Value = movieName;
            // Reading the data 

            if (movieName != null)
            {
                connection.Open();
                SqlDataReader searchMovieReader = command.ExecuteReader();


                while (searchMovieReader.Read())
                {
                    try
                    {
                        movies.Add(new Models.Movie(searchMovieReader.GetInt32(0), searchMovieReader.GetString(1), searchMovieReader.GetDateTime(2), searchMovieReader.GetString(3), Convert.ToInt32(searchMovieReader.GetDouble(4)), searchMovieReader.GetString(5), searchMovieReader.GetString(6), (byte[])searchMovieReader[7], searchMovieReader.GetInt32(8)));
                    }
                    catch (Exception e)
                    {
                        ViewBag.Error = e.Message;
                        if (HttpContext.Session.GetString("_Username") != null)
                        {
                            ViewBag.Privilege = Int32.Parse(JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Privilege")));
                        }
                        return View();
                    }
                }
                searchMovieReader.Close();
                ViewBag.movieResults = movies;
                if (HttpContext.Session.GetString("_Username") != null)
                {
                    ViewBag.Privilege = Int32.Parse(JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Privilege")));
                }
                return View();
            }
            ViewBag.Error = "Arama alanı boş bırakılamaz!";
            if (HttpContext.Session.GetString("_Username") != null)
            {
                ViewBag.Privilege = Int32.Parse(JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Privilege")));
            }
            return View();

        }

        public IActionResult GetMovie(int id)
        {
            var connection = new SqlConnection(Startup.databaseConnStr);
            var command = new SqlCommand("GetMovie", connection);
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.Parameters.Add("@movieId", System.Data.SqlDbType.Int).Value = id;
            Models.Movie selectedMovie = null;
            Models.Director selectedDirector = null;
            Models.Actor selectedActor = null;
            connection.Open();
            try
            {
                SqlDataReader getMovieReader = command.ExecuteReader();
                getMovieReader.Read();
                selectedMovie = (new Models.Movie(getMovieReader.GetInt32(0), getMovieReader.GetString(1), getMovieReader.GetDateTime(2), getMovieReader.GetString(3), Convert.ToInt32(getMovieReader.GetDouble(4)), getMovieReader.GetString(5), getMovieReader.GetString(6), (byte[])getMovieReader[7], getMovieReader.GetInt32(8)));
                selectedDirector = (new Models.Director(getMovieReader.GetString(9), getMovieReader.GetString(10), getMovieReader.GetInt32(11)));
                selectedActor = (new Models.Actor(getMovieReader.GetString(12), getMovieReader.GetString(13), getMovieReader.GetInt32(14)));
                getMovieReader.Close();
                ViewData["Movie"] = selectedMovie;
                ViewData["Director"] = selectedDirector;
                ViewData["Actor"] = selectedActor;
                if (HttpContext.Session.GetString("_Username") != null)
                {
                    ViewBag.Privilege = Int32.Parse(JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Privilege")));
                }
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                if (HttpContext.Session.GetString("_Username") != null)
                {
                    ViewBag.Privilege = Int32.Parse(JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Privilege")));
                }
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddSuccessfull(Movie_Database_System.Models.ViewModels.AddMovieVM movieVM, Movie_Database_System.Models.ViewModels.AddDirectorVM directorVM)
        {
            if (HttpContext.Session.GetString("_Username") != null)
            {
                if (Int32.Parse(JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Privilege"))) > 0)
                {
                    Movie_Database_System.Models.Movie newMovie = movieVM;
                    string summarySingleQuoted = newMovie.summary;
                    newMovie.summary = newMovie.summary.Replace("\'", "\'\'");
                    string dateAsStr = newMovie.date.ToShortDateString();
                    if (dateAsStr.Contains('.'))
                    {
                        dateAsStr = dateAsStr.Replace('.', '/');
                    }
                    else if (dateAsStr.Contains('-'))
                    {
                        dateAsStr = dateAsStr.Replace('-', '/');
                    }
                    string[] dateSplitted = dateAsStr.Split('/');
                    if (Int32.Parse(dateSplitted[1]) > 12)
                    {
                        dateAsStr = dateSplitted[1] + "/" + dateSplitted[0] + "/" + dateSplitted[2];
                    }

                    Movie_Database_System.Models.Director newDirector = directorVM;

                    if (ModelState.IsValid)
                    {
                        string dataFolderPath = "Data;MovieImages";
                        string filePath = Startup.hostEnvironment.ContentRootPath + Separate(dataFolderPath) + newMovie.image.FileName;
                        var connection = new SqlConnection(Startup.databaseConnStr);

                        /* Check if registered movie already exists */
                        List<Models.Movie> allMovies = new List<Models.Movie>();
                        bool movieExists = false;
                        int newMovieId = -1;
                        var movieCommand = new SqlCommand("getAllMovieInfo", connection);
                        movieCommand.CommandType = System.Data.CommandType.StoredProcedure;

                        try
                        {
                            connection.Open();
                            SqlDataReader movieReader = movieCommand.ExecuteReader();
                            while (movieReader.Read())
                            {
                                allMovies.Add(new Models.Movie(movieReader.GetInt32(0), movieReader.GetString(1), movieReader.GetDateTime(2), movieReader.GetString(3), (int)movieReader.GetDouble(4)));
                            }
                            movieReader.Close();

                            foreach (Models.Movie movie in allMovies)
                            {
                                if (movie.name == newMovie.name && movie.date.ToShortDateString() == dateAsStr.Replace('/', '.') && movie.genre == newMovie.genre && movie.rating == newMovie.rating)
                                {
                                    movieExists = true;
                                    newMovieId = movie.movieId;
                                    break;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            ViewBag.Error = e.Message;
                            ViewBag.Privilege = Int32.Parse(JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Privilege")));
                            return View();
                        }

                        if (!movieExists)
                        {
                            string uniqueBlobName = newMovie.image.FileName.Split(".")[0] + Guid.NewGuid().ToString() + "." + newMovie.image.FileName.Split(".")[1];
                            BlobClient blobClient = new BlobClient(
                                connectionString: Startup.blobStorageConnStr,
                                blobContainerName: "movieimages",
                                blobName: uniqueBlobName
                            );

                            var directorCommand = new SqlCommand("getAllDirectors", connection);
                            directorCommand.CommandType = System.Data.CommandType.StoredProcedure;

                            try
                            {
                                SqlDataReader directorReader = directorCommand.ExecuteReader();
                                while (directorReader.Read())
                                {
                                    directors.Add(new Movie_Database_System.Models.Director(directorReader.GetString(0), directorReader.GetString(1), directorReader.GetInt32(2), directorReader.GetInt32(3)));
                                }
                                directorReader.Close();

                                /* Upload movie image to Azure Blob Storage */
                                using (Stream filestream = new FileStream(filePath, FileMode.Create))
                                {
                                    await movieVM.image.CopyToAsync(filestream);
                                }
                                await blobClient.UploadAsync(filePath);

                                /* Upload movie metadata */
                                var command = new SqlCommand("addMovieMetadata", connection);

                                command.CommandType = System.Data.CommandType.StoredProcedure;
                                command.Parameters.Add("@trailer", System.Data.SqlDbType.NVarChar, 50).Value = newMovie.trailerURL;
                                command.Parameters.Add("@summary", System.Data.SqlDbType.NVarChar).Value = newMovie.summary;
                                command.Parameters.Add("@pictureName", System.Data.SqlDbType.NVarChar).Value = uniqueBlobName;
                                command.Parameters.Add("@sum", System.Data.SqlDbType.NVarChar).Value = summarySingleQuoted;
                                command.Parameters.Add("@newMetaId", System.Data.SqlDbType.Int);
                                command.Parameters["@newMetaId"].Direction = System.Data.ParameterDirection.Output;

                                await command.ExecuteNonQueryAsync();
                                int idMeta = (int)command.Parameters["@newMetaId"].Value;

                                /* Upload director data if submitted director is a new one */
                                bool exists = false;
                                int idDirector = -1;
                                foreach (Movie_Database_System.Models.Director director in directors)
                                {
                                    if (director.name == newDirector.name && director.surname == newDirector.surname && director.age == newDirector.age)
                                    {
                                        exists = true;
                                        idDirector = director.id;
                                        break;
                                    }
                                }

                                if (!exists)
                                {
                                    var command2 = new SqlCommand("addDirector", connection);

                                    command2.CommandType = System.Data.CommandType.StoredProcedure;
                                    command2.Parameters.Add("@name", System.Data.SqlDbType.NVarChar, 15).Value = newDirector.name;
                                    command2.Parameters.Add("@surname", System.Data.SqlDbType.NVarChar, 15).Value = newDirector.surname;
                                    command2.Parameters.Add("@age", System.Data.SqlDbType.Int).Value = newDirector.age;
                                    command2.Parameters.Add("@newId", System.Data.SqlDbType.Int);
                                    command2.Parameters["@newId"].Direction = System.Data.ParameterDirection.Output;

                                    command2.ExecuteNonQuery();
                                    idDirector = (int)command2.Parameters["@newId"].Value;
                                }

                                /* Add movie to database */
                                var command3 = new SqlCommand("addMovie", connection);

                                command3.CommandType = System.Data.CommandType.StoredProcedure;
                                command3.Parameters.Add("@name", System.Data.SqlDbType.NVarChar, 75).Value = newMovie.name;
                                command3.Parameters.Add("@date", System.Data.SqlDbType.NVarChar, 10).Value = dateAsStr;
                                command3.Parameters.Add("@genre", System.Data.SqlDbType.NVarChar, 15).Value = newMovie.genre;
                                command3.Parameters.Add("@rating", System.Data.SqlDbType.Float).Value = (float)newMovie.rating;
                                command3.Parameters.Add("@directorid", System.Data.SqlDbType.Int).Value = idDirector;
                                command3.Parameters.Add("@metadataid", System.Data.SqlDbType.Int).Value = idMeta;
                                command3.Parameters.Add("@newMovieId", System.Data.SqlDbType.Int);
                                command3.Parameters["@newMovieId"].Direction = System.Data.ParameterDirection.Output;

                                command3.ExecuteNonQuery();
                                newMovieId = (int)command3.Parameters["@newMovieId"].Value;

                                /* Update directedMovies table for the director with newly added movie */
                                var command4 = new SqlCommand("updateDirectedMovies", connection);

                                command4.CommandType = System.Data.CommandType.StoredProcedure;
                                command4.Parameters.Add("@directorid", System.Data.SqlDbType.Int).Value = idDirector;
                                command4.Parameters.Add("@movieid", System.Data.SqlDbType.Int).Value = newMovieId;

                                command4.ExecuteNonQuery();
                                connection.Close();
                            }
                            catch (Exception e)
                            {
                                ViewBag.Error = e;
                                ViewBag.Privilege = Int32.Parse(JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Privilege")));
                                return View();
                            }

                            using (var ms = new MemoryStream())
                            {
                                await newMovie.image.CopyToAsync(ms);
                                newMovie.imageBinary = ms.ToArray();
                            }
                            ViewData["NewMovie"] = newMovie;
                            ViewBag.Privilege = Int32.Parse(JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Privilege")));
                            return View();
                        }

                        else
                        {
                            ViewBag.Error = "This movie already exists in database. Please go back to try to add another movie into database.";
                            return View();
                        }
                    }

                    ViewBag.Error = "Information given to add movie was incorrect. If you wish to add it again, please go back and fill the form correctly.";
                    return View();
                }
                else
                {
                    ViewBag.Error = "You don't have authorization required to add a movie and it's connected data.";
                    return View();
                }
            }
            else
            {
                ViewBag.Error = "You don't have authorization required to add a movie and it's connected data.";
                return View();
            }
            
        }

        public IActionResult ActorsInMovie(int id)
        {
            String movieName = "";
            List<Models.Actor> actorsInMovie = new List<Models.Actor>();
            var connection = new SqlConnection(Startup.databaseConnStr);

            try
            {
                var command = new SqlCommand("getActorsInMovie", connection);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add("@movieId", System.Data.SqlDbType.Int).Value = id;

                connection.Open();
                SqlDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    movieName = dataReader.GetString(0);
                    Models.Actor actor = new Models.Actor();
                    actor.name = dataReader.GetString(1);
                    actor.surname = dataReader.GetString(2);
                    actor.id = dataReader.GetInt32(3);
                    actor.age = 0;

                    actorsInMovie.Add(actor);
                }

                dataReader.Close();
                connection.Close();

            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message;
                if (HttpContext.Session.GetString("_Username") != null)
                {
                    ViewBag.Privilege = Int32.Parse(JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Privilege")));
                }
                return View();
            }

            ViewData["movieName"] = movieName;
            ViewData["actorsInMovie"] = actorsInMovie;
            if (HttpContext.Session.GetString("_Username") != null)
            {
                ViewBag.Privilege = Int32.Parse(JsonSerializer.Deserialize<string>(HttpContext.Session.GetString("_Privilege")));
            }
            return View();
        }

    }
}
