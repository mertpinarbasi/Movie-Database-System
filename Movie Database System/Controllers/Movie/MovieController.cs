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
        private IConfiguration _config;
        private List<Movie_Database_System.Models.Director> directors;

        public MovieController(IConfiguration config)
        {
            _config = config;
            directors = new List<Movie_Database_System.Models.Director>();
        }

        public IActionResult AddMovie()
        {
            try
            {
                var connection = new SqlConnection(Startup.databaseConnString);
                var command = new SqlCommand("getAllDirectors", connection);
                command.CommandType = System.Data.CommandType.StoredProcedure;

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    directors.Add(new Movie_Database_System.Models.Director(reader.GetString(0), reader.GetString(1), reader.GetInt32(2), reader.GetInt32(3)));
                }

                ViewData["DirectorList"] = directors;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("-------------------------");
                Console.WriteLine(e.StackTrace);
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddMovie(Movie_Database_System.Models.ViewModels.AddMovieVM movieVM, Movie_Database_System.Models.ViewModels.AddDirectorVM directorVM)
        {
            Movie_Database_System.Models.Movie newMovie = movieVM;
            Movie_Database_System.Models.Director newDirector = directorVM;
            string filePath = Startup.hostEnvironment.ContentRootPath + "\\Data\\MovieImages\\" + newMovie.image.FileName;
            var connection = new SqlConnection(Startup.databaseConnString);

            string uniqueBlobName = newMovie.image.FileName.Split(".")[0] + Guid.NewGuid().ToString() + "." + newMovie.image.FileName.Split(".")[1];
            BlobClient blobClient = new BlobClient(
                connectionString: Startup.blobStorageConnString,
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

                await command.ExecuteNonQueryAsync();

                /* Upload director data if submitted director is a new one */
                bool exists = false;
                foreach (Movie_Database_System.Models.Director director in directors)
                {
                    if (director.name == newDirector.name && director.surname == newDirector.surname && director.age == newDirector.age)
                    {
                        exists = true;
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

                    command2.ExecuteNonQuery();
                }

                /* Get new (or existing) director's ID to add it to movie table with new movie (Foreign key) */
                var command3 = new SqlCommand("getDirector", connection);

                command3.CommandType = System.Data.CommandType.StoredProcedure;
                command3.Parameters.Add("@name", System.Data.SqlDbType.NVarChar, 15).Value = newDirector.name;
                command3.Parameters.Add("@surname", System.Data.SqlDbType.NVarChar, 15).Value = newDirector.surname;
                command3.Parameters.Add("@age", System.Data.SqlDbType.Int).Value = newDirector.age;

                int idDirector = -1;
                SqlDataReader reader = command3.ExecuteReader();
                if (reader.Read())
                {
                    idDirector = reader.GetInt32(0);
                }
                else
                {
                    throw new Exception("New director wasn't found in database. Something went wrong!");
                }
                reader.Close();

                /* Get metadata id */
                var command4 = new SqlCommand("getMovieMetadata", connection);

                command4.CommandType = System.Data.CommandType.StoredProcedure;
                command4.Parameters.Add("@trailer", System.Data.SqlDbType.NVarChar, 50).Value = newMovie.trailerURL;
                command4.Parameters.Add("@summary", System.Data.SqlDbType.NVarChar).Value = newMovie.summary;

                int idMeta = -1;
                SqlDataReader reader2 = command4.ExecuteReader();
                if (reader2.Read())
                {
                    idMeta = reader2.GetInt32(0);
                }
                else
                {
                    throw new Exception("New metadata wasn't found in database. Something went wrong!");
                }
                reader2.Close();

                /* Add movie to database */
                var command5 = new SqlCommand("addMovie", connection);

                command5.CommandType = System.Data.CommandType.StoredProcedure;
                command5.Parameters.Add("@name", System.Data.SqlDbType.NVarChar, 30).Value = newMovie.name;
                command5.Parameters.Add("@date", System.Data.SqlDbType.NVarChar, 10).Value = newMovie.date;
                command5.Parameters.Add("@genre", System.Data.SqlDbType.NVarChar, 15).Value = newMovie.genre;
                command5.Parameters.Add("@rating", System.Data.SqlDbType.Float).Value = (float)newMovie.rating;
                command5.Parameters.Add("@directorid", System.Data.SqlDbType.Int).Value = idDirector;
                command5.Parameters.Add("@metadataid", System.Data.SqlDbType.Int).Value = idMeta;

                command5.ExecuteNonQuery();
                connection.Close();

                System.IO.File.Delete(filePath);
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
            }

            return Json(newMovie);
        }

    }
}
