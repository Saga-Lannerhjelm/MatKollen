using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MatKollen.Models;
using MatKollen.Services;
using MatKollen.ViewModels;
using MySql.Data.MySqlClient;

namespace MatKollen.DAL.Repositories
{
    public class RecipeRepository
    {
        private readonly string _connectionString;
        private readonly ConvertQuantityHandler _convertQuantityHandler;
        
        public RecipeRepository(IConfiguration configuration, ConvertQuantityHandler convertQuantityHandler)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _convertQuantityHandler = convertQuantityHandler;
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

        public List<RecipeDetailsViewModel>? GetUsersRecipes(int userId, out string errorMsg)
        {
            var myConnectionString = _connectionString;

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                string query  = "SELECT * FROM vw_recipe_details WHERE user_id = @userId";
                var recipeList = new List<RecipeDetailsViewModel>();

                try
                {
                    // create a MySQL command and set the SQL statement with parameters
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);
                    myConnection.Open();
                    myCommand.Parameters.Add("@userId", MySqlDbType.Int32).Value = userId;

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

                            if (!reader.IsDBNull(reader.GetOrdinal("quantity")))
                            {
                                recipe.Ingredients.Add(new RecipeIngredientViewModel()
                                {
                                    IngredientDetails = new RecipeFoodItem()
                                    {
                                        Id = reader.GetInt32("ingredient_id"),
                                        Quantity = reader.GetDecimal("quantity"),
                                        UnitId =  reader.GetInt32("unit_id"),
                                        FoodItemId = reader.GetInt32("food_item_id")
                                    },
                                    ConvertedQuantity = _convertQuantityHandler.ConverFromtLiterOrKg(reader.GetDecimal("quantity"), reader.GetDouble("conversion_multiplier")),
                                    Multiplier = reader.GetDouble("conversion_multiplier"),
                                    Type = reader.GetString("type"),
                                    Ingredient = reader.GetString("ingredient"),
                                    Unit = reader.GetString("unit")
                                }); 
                            }
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

        public int InsertRecipe(Recipe recipe, out string errorMsg)
        {
            var myConnectionString = _connectionString;

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                string query  = "INSERT INTO recipes (title, description, recipe_category_id, user_id) VALUES (@title, @description, @recipeCategoryId, @userId); SELECT LAST_INSERT_ID();";
                try
                {
                    // create a MySQL command and set the SQL statement with parameters
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);
                    myConnection.Open();

                    myCommand.Parameters.Add("@title", MySqlDbType.VarChar, 50).Value = recipe.Title;
                    myCommand.Parameters.Add("@description", MySqlDbType.VarChar, 1000).Value = recipe.Description;
                    myCommand.Parameters.Add("@recipeCategoryId", MySqlDbType.Int32).Value = recipe.RecipeCategoryId;
                    myCommand.Parameters.Add("@userId", MySqlDbType.Int32).Value = recipe.UserId;

                    errorMsg = "";

                    int lastInsertedId = Convert.ToInt32(myCommand.ExecuteScalar());
                    
                    return lastInsertedId;
                }
                catch (MySqlException e)
                {
                    errorMsg = e.Message;
                    return 0;
                }    
            }
        }

        public int InsertIngredient(RecipeFoodItem ingredient, out string errorMsg)
        {
            var myConnectionString = _connectionString;

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                string query  = "INSERT INTO recipe_has_fooditems (quantity, unit_id, recipe_id, food_item_id) VALUES (@quantity, @unitId, @recipeId, @foodItemId); SELECT LAST_INSERT_ID();";
                try
                {
                    // create a MySQL command and set the SQL statement with parameters
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);
                    myConnection.Open();

                    myCommand.Parameters.Add("@quantity", MySqlDbType.Decimal).Value = ingredient.Quantity;
                    myCommand.Parameters["@quantity"].Precision = 9;
                    myCommand.Parameters["@quantity"].Scale = 4;
                    myCommand.Parameters.Add("@unitId", MySqlDbType.VarChar, 1000).Value = ingredient.UnitId;
                    myCommand.Parameters.Add("@recipeId", MySqlDbType.Int32).Value = ingredient.RecipeId;
                    myCommand.Parameters.Add("@foodItemId", MySqlDbType.Int32).Value = ingredient.FoodItemId;

                    errorMsg = "";

                    int affectedRows = myCommand.ExecuteNonQuery();
                    
                    return affectedRows;
                }
                catch (MySqlException e)
                {
                    errorMsg = e.Message;
                    return 0;
                }    
            }
        }

        public int Update(Recipe recipe, out string errorMsg)
        {
            var myConnectionString = _connectionString;

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                string query  = "UPDATE `recipes` SET `title` = @title, `description` = @description, `recipe_category_id` = @categoryId WHERE `id` = @recipeId";

                try
                {
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);
                    myConnection.Open();

                    myCommand.Parameters.Add("@recipeId", MySqlDbType.Int32).Value = recipe.Id;
                    myCommand.Parameters.Add("@title", MySqlDbType.VarChar, 50).Value = recipe.Title;
                    myCommand.Parameters.Add("@description", MySqlDbType.VarChar, 1000).Value = recipe.Description;
                    myCommand.Parameters.Add("@categoryId", MySqlDbType.Int32).Value = recipe.RecipeCategoryId;

                    errorMsg = "";

                    var rowsAffected = myCommand.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        errorMsg = "Gick inte att updatera receptet.";
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
        
        public int UpdateQuantity(int id, decimal nr, out string errorMsg)
        {
            var myConnectionString = _connectionString;

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                string query  = "Update recipe_has_fooditems SET quantity = ROUND((quantity + @nr), 4) WHERE id = @id";

                try
                {
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);
                    myConnection.Open();

                    myCommand.Parameters.Add("@nr", MySqlDbType.Decimal).Value = nr;
                    myCommand.Parameters.Add("@id", MySqlDbType.Int32).Value = id;

                    errorMsg = "";

                    var rowsAffected = myCommand.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        errorMsg = "Gick inte att updatera antalet.";
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

        public int Delete(int recipeId, out string errorMsg)
        {
            var myConnectionString = _connectionString;

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                //Creates a new user and a grozery list for that user at the same time
                string query  = "DELETE FROM recipes WHERE id = @recipeId;";

                try
                {
                    // create a MySQL command and set the SQL statement with parameters
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);

                    //open a connection
                    myConnection.Open();

                    myCommand.Parameters.Add("@recipeId", MySqlDbType.Int32).Value = recipeId;

                    errorMsg = "";

                    // execute the command and read the results
                    var rowsAffected = myCommand.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        errorMsg = "Inget recept togs bort";
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
        public int DeleteIngredient(int ingredientId, out string errorMsg)
        {
            var myConnectionString = _connectionString;

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                //Creates a new user and a grozery list for that user at the same time
                string query  = "DELETE FROM recipe_has_fooditems WHERE id = @ingredientId;";

                try
                {
                    // create a MySQL command and set the SQL statement with parameters
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);

                    //open a connection
                    myConnection.Open();

                    myCommand.Parameters.Add("@ingredientId", MySqlDbType.Int32).Value = ingredientId;

                    errorMsg = "";

                    // execute the command and read the results
                    var rowsAffected = myCommand.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        errorMsg = "Ingen ingrediens togs bort";
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
    }
}