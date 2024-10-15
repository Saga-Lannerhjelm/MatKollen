using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MatKollen.Models;
using MySql.Data.MySqlClient;

namespace MatKollen.DAL.Repositories
{
    public class AccountRepository
    {

        private readonly string _connectionString;
        
        public AccountRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public int InertUser(User user, out string errorMsg)
        {
            var myConnectionString = _connectionString;

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                string query  = "INSERT INTO users (username, email, password) VALUES (@username, @email, @password)";
                var testList = new List<string>();

                try
                {
                    // create a MySQL command and set the SQL statement with parameters
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);

                    //open a connection
                    myConnection.Open();

                    myCommand.Parameters.Add("@username", MySqlDbType.VarChar, 50).Value = user.Username;
                    myCommand.Parameters.Add("@email", MySqlDbType.VarChar, 320).Value = user.Email;
                    myCommand.Parameters.Add("@password", MySqlDbType.Binary, 32).Value = user.PasswordHashed;

                    errorMsg = "";

                    // execute the command and read the results
                    var rowsAffected = myCommand.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        errorMsg = "Gick inte att lägga till användaren";
                        return 0;
                    }
                    
                    return rowsAffected;
                }
                catch (MySqlException e)
                {
                    errorMsg = e.Message;
                    return 0;
                }    

            }
        }

        public User? GetUserCredentials(User user, out string errorMsg)
        {
            var myConnectionString = _connectionString;

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                string query  = "SELECT id, username, password FROM users WHERE username = @username;";
                var userCredentials = new User();

                try
                {
                    // create a MySQL command and set the SQL statement with parameters
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);

                    //open a connection
                    myConnection.Open();

                    myCommand.Parameters.AddWithValue("@username", user.Username);

                    errorMsg = "";

                    // execute the command and read the results
                    using var myReader = myCommand.ExecuteReader();
                    {
                        while (myReader.Read())
                        {
                            userCredentials.Id = myReader.GetInt32("id");
                            userCredentials.Username = myReader.GetString("username");
                            userCredentials.PasswordHashed = (byte[])myReader["password"];
                        }
                    }
                    if (userCredentials.PasswordHashed == null)
                    {
                        errorMsg = "Fel användarnamn eller lösenord";
                        return null;
                    }
                    return userCredentials;
                }
                catch (MySqlException e)
                {
                    errorMsg = e.Message;
                    return null;
                }    
            }
        }
    }
}