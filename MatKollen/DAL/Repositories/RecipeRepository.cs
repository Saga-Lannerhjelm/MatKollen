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
        private readonly string _connectionString;
        
        public RecipeRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public List<RecipeDetailsViewModel>? GetRecipes( out string errorMsg)
        {
            var myConnectionString = _connectionString;

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
                                    Id = reader.GetInt32("id"),
                                    Title = reader.GetString("title"),
                                    Description = reader.GetString("description"),
                                    CreatedAt = reader.GetDateTime("created_at"),
                                    RecipeCategoryId = reader.GetInt32("recipe_category_id"),
                                    UserId = reader.GetInt32("user_id")
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

        public RecipeDetailsViewModel? GetRecipe(int recipeId, out string errorMsg)
        {
            var myConnectionString = _connectionString;

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                string query  = "SELECT * FROM vw_recipe_with_ingredients WHERE id = @recipeId";
                var recipe = new RecipeDetailsViewModel();

                try
                {
                    // create a MySQL command and set the SQL statement with parameters
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);
                    myConnection.Open();

                    myCommand.Parameters.AddWithValue("@recipeId", recipeId);

                    errorMsg = "";

                    using var reader = myCommand.ExecuteReader();
                    {

                        while (reader.Read())
                        {

                            if (recipe.Recipe == null)
                            {
                                recipe.Recipe = new Recipe()
                                {
                                    Id = reader.GetInt32("id"),
                                    Title = reader.GetString("title"),
                                    Description = reader.GetString("description"),
                                    CreatedAt = reader.GetDateTime("created_at"),
                                    RecipeCategoryId = reader.GetInt32("recipe_category_id"),
                                    UserId = reader.GetInt32("user_id")
                                };
                                recipe.Category = reader.GetString("category");
                                recipe.Username = reader.GetString("username"); 
                            }

                            recipe.Ingredients.Add(new RecipeIngredientViewModel()
                            {
                                IngredientDetails = new RecipeFoodItem()
                                {
                                    Quantity = reader.GetInt32("quantity"),
                                    UnitId =  reader.GetInt32("unit_id"),
                                    FoodItemId = reader.GetInt32("food_item_id")
                                },
                                Ingredient = reader.GetString("ingredient"),
                                Unit = reader.GetString("unit")
                            });
                        }
                    }
                    return recipe;
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