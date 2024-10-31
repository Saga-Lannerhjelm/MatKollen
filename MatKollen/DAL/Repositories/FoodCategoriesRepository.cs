using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatKollen.Models;
using MySql.Data.MySqlClient;

namespace MatKollen.DAL.Repositories
{
    public class FoodCategoriesRepository
    {
         private readonly string _connectionString;
        
        public FoodCategoriesRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public List<FoodCategory> GetFoodCategories(out string errorMsg)
        {
            var myConnectionString = _connectionString;
            string query  = "SELECT id, name FROM food_categories";
            List<FoodCategory> categoryList = [];

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                try
                {
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);
                    myConnection.Open();
                    errorMsg = "";

                    using var reader = myCommand.ExecuteReader();
                    {
                        while (reader.Read())
                        {
                            var unit = new FoodCategory()
                            {
                                Id = reader.GetInt32("id"),
                                Name = reader.GetString("name"),
                            };
                            categoryList.Add(unit);
                        }
                    }
                    return categoryList;
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