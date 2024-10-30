using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MatKollen.Models;
using MatKollen.Services;
using MatKollen.ViewModels;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Bcpg;

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
        public List<RecipeDetailsViewModel>? GetRecipes(string searchPrompt, int categoryId, out string errorMsg)
        {
            var myConnectionString = _connectionString;

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                string query  = "SELECT * FROM vw_recipe_details";
                if (searchPrompt != null || categoryId != 0)
                {
                    query += " WHERE";
                }
                if (!string.IsNullOrEmpty(searchPrompt)) 
                {
                    query += " title LIKE @searchPrompt";
                    if (categoryId != 0) query += " AND";
                }
                if (categoryId != 0) query += " recipe_category_id = @category";
                var recipeList = new List<RecipeDetailsViewModel>();

                try
                {
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);
                    myConnection.Open();
                    myCommand.Parameters.Add("@searchPrompt", MySqlDbType.VarChar, 50).Value = "%" + searchPrompt + "%";
                    myCommand.Parameters.Add("@category", MySqlDbType.Int32).Value = categoryId;

                    errorMsg = "";

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
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);
                    myConnection.Open();
                    myCommand.Parameters.Add("@userId", MySqlDbType.Int32).Value = userId;

                    errorMsg = "";

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

        public RecipeDetailsViewModel? GetRecipe(int recipeId, int userId, out string errorMsg)
        {
            var myConnectionString = _connectionString;

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                string query  = "SELECT " +
                                "recipes.*, rc.name AS category, users.username, " +
                                "CASE " + 
                                    "WHEN rhf.food_item_id IN " + 
                                        "(SELECT food_item_id FROM user_has_fooditems WHERE user_id = @userId) " + 
                                    "THEN 1 ELSE 0 " +
                                "END AS user_has_item, " +
                                "CASE " + 
                                    "WHEN ms.type IN " +
                                        "(SELECT ms2.type FROM user_has_fooditems AS uhf INNER JOIN measurement_units AS ms2 ON ms2.id = uhf.unit_id WHERE uhf.user_id = @userId AND uhf.food_item_id = rhf.food_item_id) " + 
                                    "THEN 1 ELSE 0 " +
                                "END AS exists_in_same_type, " +
                                "CASE " + 
                                    "WHEN rhf.quantity <= " + 
                                        "(SELECT SUM(quantity) FROM user_has_fooditems AS uhf " + 
                                        "INNER JOIN measurement_units AS ms ON ms.id = uhf.unit_id " +
                                        "WHERE food_item_id = rhf.food_item_id " +
                                        "AND user_id = @userId) " + 
                                    "THEN 1 ELSE 0 " +
                                "END AS quantity_exists, rhf.id AS ingredient_id, rhf.quantity, rhf.food_item_id, rhf.unit_id,  ms.unit, ms.conversion_multiplier, ms.`type`, fi.name AS ingredient " +
                                "FROM recipes " +
                                "LEFT JOIN recipe_has_fooditems AS rhf ON rhf.recipe_id = recipes.id " +
                                "LEFT JOIN measurement_units AS ms ON ms.id = rhf.unit_id " +
                                "LEFT JOIN recipe_categories AS rc ON rc.id = recipes.recipe_category_id " +
                                "LEFT JOIN users ON users.id = recipes.user_id " +
                                "LEFT JOIN food_items AS fi ON fi.id = rhf.food_item_id " +
                                "WHERE recipes.id = @recipeId";

                var recipe = new RecipeDetailsViewModel();

                try
                {
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);
                    myConnection.Open();

                    myCommand.Parameters.Add("@recipeId", MySqlDbType.Int32).Value = recipeId;
                    myCommand.Parameters.Add("@userId", MySqlDbType.Int32).Value = userId;

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
                                    UnitInfo = new MeasurementUnit()
                                    {
                                        Unit = reader.GetString("unit"),
                                        Multiplier = reader.GetDouble("conversion_multiplier"),
                                        Type = reader.GetString("type"),
                                    },
                                    ConvertedQuantity = _convertQuantityHandler.ConvertFromLiterOrKg(reader.GetDecimal("quantity"), reader.GetDouble("conversion_multiplier")),
                                    Ingredient = reader.GetString("ingredient"),
                                    UserHasIngredient = reader.GetBoolean("user_has_item"),
                                    QuantityExists = reader.GetBoolean("quantity_exists"),
                                    IngredientExistInSameType = reader.GetBoolean("exists_in_same_type"),
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
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);
                    myConnection.Open();

                    myCommand.Parameters.Add("@title", MySqlDbType.VarChar, 50).Value = recipe.Title;
                    myCommand.Parameters.Add("@description", MySqlDbType.VarChar, 1000).Value = recipe.Description;
                    myCommand.Parameters.Add("@recipeCategoryId", MySqlDbType.Int32).Value = recipe.RecipeCategoryId;
                    myCommand.Parameters.Add("@userId", MySqlDbType.Int32).Value = recipe.UserId;

                    errorMsg = "";
                    int lastInsertedId = 0;

                    lastInsertedId = Convert.ToInt32(myCommand.ExecuteScalar());
                    
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
                string query  = "INSERT INTO recipe_has_fooditems (quantity, unit_id, recipe_id, food_item_id) VALUES (@quantity, @unitId, @recipeId, @foodItemId);";
                try
                {
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
        
        public double UpdateQuantity(int id, decimal nr, out string errorMsg)
        {
            var myConnectionString = _connectionString;

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                string query  = "CALL sp_update_recipe_food_item_quantity(@id, @nr)";

                try
                {
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);
                    myConnection.Open();

                    myCommand.Parameters.Add("@id", MySqlDbType.Int32).Value = id;
                    myCommand.Parameters.Add("@nr", MySqlDbType.Decimal).Value = nr;

                    errorMsg = "";

                    var reader = myCommand.ExecuteReader();
                    double updatedQuantity = -1;

                    while (reader.Read())
                    {
                        var test = reader.GetDecimal("quantity");
                        updatedQuantity = Convert.ToDouble(_convertQuantityHandler.ConvertFromLiterOrKg(reader.GetDecimal("quantity"), reader.GetDouble("conversion_multiplier")));
                    }
                    
                    return updatedQuantity;
                }
                catch (MySqlException e)
                {
                    errorMsg = e.Message;
                    return -1;
                }    
            }
        }

        public int DeleteIngredient(int ingredientId, out string errorMsg)
        {
            var myConnectionString = _connectionString;

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                string query  = "DELETE FROM recipe_has_fooditems WHERE id = @ingredientId;";

                try
                {
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);
                    myConnection.Open();

                    myCommand.Parameters.Add("@ingredientId", MySqlDbType.Int32).Value = ingredientId;

                    errorMsg = "";

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

        public int Delete(int recipeId, out string errorMsg)
        {
            var myConnectionString = _connectionString;

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                string query  = "DELETE FROM recipes WHERE id = @recipeId;";

                try
                {
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);
                    myConnection.Open();

                    myCommand.Parameters.Add("@recipeId", MySqlDbType.Int32).Value = recipeId;

                    errorMsg = "";

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
    }
}