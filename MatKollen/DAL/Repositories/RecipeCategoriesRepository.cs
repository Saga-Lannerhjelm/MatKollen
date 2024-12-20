using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatKollen.Models;
using MySql.Data.MySqlClient;

namespace MatKollen.DAL.Repositories
{
    public class RecipeCategoriesRepository
    {
         private readonly string _connectionString;
        
        public RecipeCategoriesRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public List<RecipeCategory> GetRecipeCategories(out string errorMsg)
        {
            var myConnectionString = _connectionString;
            string query  = "SELECT id, name FROM recipe_categories";
            List<RecipeCategory> categoryList = [];

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
                            var category = new RecipeCategory()
                            {
                                Id = reader.GetInt32("id"),
                                Name = reader.GetString("name"),
                            };
                            categoryList.Add(category);
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