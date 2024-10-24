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
        public int InsertUser(User user, out string errorMsg)
        {
            var myConnectionString = _connectionString;

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                //Creates a new user and a grozery list for that user at the same time
                string query  = "INSERT INTO users (username, email, password) VALUES (@username, @email, @password); INSERT INTO lists (name, user_id) VALUES (@groceryListName, LAST_INSERT_ID())";
                var testList = new List<string>();

                try
                {
                    // create a MySQL command and set the SQL statement with parameters
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);

                    //open a connection
                    myConnection.Open();

                    myCommand.Parameters.Add("@username", MySqlDbType.VarChar, 50).Value = user.Username;
                    myCommand.Parameters.Add("@email", MySqlDbType.VarChar, 320).Value = user.Email;
                    myCommand.Parameters.Add("@password", MySqlDbType.VarChar, 500).Value = user.Password;
                    myCommand.Parameters.Add("@groceryListName", MySqlDbType.VarChar, 50).Value = user.Username + "s Inköpslista";

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
                            userCredentials.Password = myReader.GetString("password");
                        }
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