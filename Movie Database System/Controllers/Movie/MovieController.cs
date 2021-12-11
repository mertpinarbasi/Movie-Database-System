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
                Console.WriteLine(e.Message);
                Console.WriteLine("-------------------------");
                Console.WriteLine(e.StackTrace);
            }

            return View();
        }
        public IActionResult SearchMovie()
        {
            return View();
        }
        [HttpPost]
        public IActionResult SearchMovie(string movieName)
        {
            var connection = new SqlConnection(Startup.databaseConnStr);
            var command = new SqlCommand("Search_Movie", connection);
            List<Movie_Database_System.Models.Movie> movies = new List<Movie_Database_System.Models.Movie>();
            List<String> movieNameList = new List<String>();
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.Parameters.Add("@searchInput", System.Data.SqlDbType.NVarChar).Value = movieName;
            // Reading the data 
            connection.Open();
            SqlDataReader searchMovieReader = command.ExecuteReader();
            while (searchMovieReader.Read())
            {
                //movies.Add(new Movie_Database_System.Models.Movie(searchMovieReader.GetInt32(0), searchMovieReader.GetString(1), searchMovieReader.GetString(2), searchMovieReader.GetString(3), searchMovieReader.GetInt32(4)));
                movieNameList.Add(searchMovieReader.GetString(0));
            }
            searchMovieReader.Close();

            return Json(movieNameList);
        }

        [HttpPost]

        public async Task<IActionResult> AddMovie(Movie_Database_System.Models.ViewModels.AddMovieVM movieVM, Movie_Database_System.Models.ViewModels.AddDirectorVM directorVM)
        {
            Movie_Database_System.Models.Movie newMovie = movieVM;
            string summarySingleQuoted = newMovie.summary;
            newMovie.summary = newMovie.summary.Replace("\'", "\'\'");
            Movie_Database_System.Models.Director newDirector = directorVM;

            string dataFolderPath = "Data;MovieImages";
            string filePath = Startup.hostEnvironment.ContentRootPath + Separate(dataFolderPath) + newMovie.image.FileName;
            var connection = new SqlConnection(Startup.databaseConnStr);

            string uniqueBlobName = newMovie.image.FileName.Split(".")[0] + Guid.NewGuid().ToString() + "." + newMovie.image.FileName.Split(".")[1];
            BlobClient blobClient = new BlobClient(
                connectionString: Startup.blobStorageConnStr,
                blobContainerName: "movieimages",
                blobName: uniqueBlobName
            );

            var directorCommand = new SqlCommand("getAllDirectors", connection);
            directorCommand.CommandType = System.Data.CommandType.StoredProcedure;

            connection.Open();
            SqlDataReader directorReader = directorCommand.ExecuteReader();
            while (directorReader.Read())
            {
                directors.Add(new Movie_Database_System.Models.Director(directorReader.GetString(0), directorReader.GetString(1), directorReader.GetInt32(2), directorReader.GetInt32(3)));
            }
            directorReader.Close();

            try
            {
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
                command3.Parameters.Add("@name", System.Data.SqlDbType.NVarChar, 30).Value = newMovie.name;
                command3.Parameters.Add("@date", System.Data.SqlDbType.NVarChar, 10).Value = newMovie.date;
                command3.Parameters.Add("@genre", System.Data.SqlDbType.NVarChar, 15).Value = newMovie.genre;
                command3.Parameters.Add("@rating", System.Data.SqlDbType.Float).Value = (float)newMovie.rating;
                command3.Parameters.Add("@directorid", System.Data.SqlDbType.Int).Value = idDirector;
                command3.Parameters.Add("@metadataid", System.Data.SqlDbType.Int).Value = idMeta;
                command3.Parameters.Add("@newMovieId", System.Data.SqlDbType.Int);
                command3.Parameters["@newMovieId"].Direction = System.Data.ParameterDirection.Output;

                command3.ExecuteNonQuery();
                int newMovieId = (int)command3.Parameters["@newMovieId"].Value;

                /* Update directedMovies table for the director with newly added movie */
                var command4 = new SqlCommand("updateDirectedMovies", connection);

                command4.CommandType = System.Data.CommandType.StoredProcedure;
                command4.Parameters.Add("@directorid", System.Data.SqlDbType.Int).Value = idDirector;
                command4.Parameters.Add("@movieid", System.Data.SqlDbType.Int).Value = newMovieId;

                command4.ExecuteNonQuery();

                connection.Close();
                System.IO.File.Delete(filePath);

            }
            catch (Exception err)
            {
                return Json(err);
            }

            return Json(newMovie);
        }
    }
}
