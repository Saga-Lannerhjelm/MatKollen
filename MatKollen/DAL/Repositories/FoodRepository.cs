using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatKollen.Services;
using MySql.Data.MySqlClient;
using Mysqlx;

namespace MatKollen.Controllers.Repositories
{
    public class FoodRepository
    {
        public List<string> GetData(out string errorMsg)
        {
            var myConnectionString = "Server=localhost;Database=MatKollen;Uid=root;Pwd=mySqlPw123;";

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                string query  = "SELECT * FROM food_categories;";
                var testList = new List<string>();

                try
                {
                    // create a MySQL command and set the SQL statement with parameters
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);

                    //open a connection
                    myConnection.Open();

                    // myCommand.Parameters.AddWithValue("@clientId", clientId);

                    errorMsg = "";

                    // execute the command and read the results
                    using var myReader = myCommand.ExecuteReader();
                    {
                        while (myReader.Read())
                        {
                            string test = myReader.GetString("name");
                            testList.Add(test);
                        }
                    }
                    return testList;
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