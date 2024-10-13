using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatKollen.Models;
using MatKollen.ViewModels;
using MySql.Data.MySqlClient;

namespace MatKollen.DAL.Repositories
{
    public class RecipeRepository
    {
        public List<RecipeDetailsViewModel>? GetRecipes( out string errorMsg)
        {
            var myConnectionString = "Server=localhost;Database=MatKollen;Uid=root;Pwd=mySqlPw123;";

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                string query  = "SELECT * FROM vw_recipe_details";
                var recipeList = new List<RecipeDetailsViewModel>();

                try
                {
                    // create a MySQL command and set the SQL statement with parameters
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);
                    myConnection.Open();

                    errorMsg = "";

                    // execute the command and read the results
                    using var reader = myCommand.ExecuteReader();
                    {
                        while (reader.Read())
                        {
                            var recipe = new RecipeDetailsViewModel()
                            {
                                Recipe = new Recipe()
                                {
                                    Title = reader.GetString("title"),
                                    Description = reader.GetString("description"),
                                    CreatedAt = reader.GetDateTime("created_at"),
                                },
                                Category = reader.GetString("category"),
                                Username = reader.GetString("username"),
                            }; 
                            recipeList.Add(recipe);
                        }
                    }
                    return recipeList;
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