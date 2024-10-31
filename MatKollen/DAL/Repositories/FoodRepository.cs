using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using MatKollen.Models;
using MatKollen.Services;
using MatKollen.ViewModels;
using MySql.Data.MySqlClient;
using Mysqlx;

namespace MatKollen.Controllers.Repositories
{
    public class FoodRepository
    {
        private readonly string _connectionString;
        private readonly ConvertQuantityHandler _convertQuantityHandler;
        
        public FoodRepository(IConfiguration configuration, ConvertQuantityHandler convertQuantityHandler)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");;
            _convertQuantityHandler = convertQuantityHandler;
        }
        

        public List<FoodItem>? GetFoodItems (out string errorMsg)
        {
            var myConnectionString = _connectionString;
            string query  = "SELECT * FROM food_items";
            var foodItems = new List<FoodItem>();

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
                            var foodItem = new FoodItem()
                            {
                                Id = reader.GetInt32("id"),
                                Name = reader.GetString("name"),
                                FoodCategoryId = reader.GetInt32("food_category_id")
                            };
                            foodItems.Add(foodItem);
                        }
                    }
                    return foodItems;
                }
                catch (MySqlException e)
                {
                    errorMsg = e.Message;
                    return null;
                }    
            }
        }

        public int InsertFoodAndAssignToUserInventory(FoodAndUserFoodItemViewModel foodItem, out string errorMsg)
        {
            var myConnectionString = _connectionString;

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                string query  = "CALL `insert_food_and_assign_to_user`(@foodName, @categoryId, @quantity, @expirationDate, @userId, @unitId);";

                try
                {
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);

                    myConnection.Open();

                    myCommand.Parameters.Add("@foodName", MySqlDbType.VarChar, 50).Value = foodItem.FoodItem.Name;
                    myCommand.Parameters.Add("@categoryId", MySqlDbType.Int32).Value = foodItem.FoodItem.FoodCategoryId;
                    myCommand.Parameters.Add("@quantity", MySqlDbType.Decimal).Value = foodItem.UserFoodItem.Quantity;
                    myCommand.Parameters.Add("@expirationDate", MySqlDbType.Date).Value = foodItem.UserFoodItem.ExpirationDate;
                    myCommand.Parameters.Add("@userId", MySqlDbType.Int32).Value = foodItem.UserFoodItem.UserId;
                    myCommand.Parameters.Add("@unitId", MySqlDbType.Int32).Value = foodItem.UserFoodItem.UnitId;

                    errorMsg = "";

                    var rowsAffected = Convert.ToInt32(myCommand.ExecuteScalar());

                    if (rowsAffected == 0)
                    {
                        errorMsg = "Ingen matvara lades till.";
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

        public int InsertFoodAndAssignToRecipe(IngredientAndFoodItemViewModel foodItem, out string errorMsg)
        {
            var myConnectionString = _connectionString;

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                string query  = "CALL `insert_food_and_assign_to_recipe`(@foodName, @categoryId, @quantity, @recipeId, @unitId);";
                var testList = new List<string>();

                try
                {
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);

                    myConnection.Open();

                    myCommand.Parameters.Add("@foodName", MySqlDbType.VarChar, 50).Value = foodItem.FoodItem.Name;
                    myCommand.Parameters.Add("@categoryId", MySqlDbType.Int32).Value = foodItem.FoodItem.FoodCategoryId;
                    myCommand.Parameters.Add("@quantity", MySqlDbType.Decimal).Value = foodItem.RecipeFoodItem.Quantity;
                    myCommand.Parameters.Add("@recipeId", MySqlDbType.Int32).Value = foodItem.RecipeFoodItem.RecipeId;
                    myCommand.Parameters.Add("@unitId", MySqlDbType.Int32).Value = foodItem.RecipeFoodItem.UnitId;

                    errorMsg = "";

                    var rowsAffected = Convert.ToInt32(myCommand.ExecuteScalar());

                    if (rowsAffected == 0)
                    {
                        errorMsg = "Ingen matvara lades till.";
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

         public int InsertFoodAndAssignToGroceryList(FoodAndGroceryListFoodViewModel foodItem, out string errorMsg)
        {
            var myConnectionString = _connectionString;

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                string query  = "CALL `insert_food_and_assign_to_grocery_list`(@foodName, @categoryId, @quantity, @listId, @unitId);";
                var testList = new List<string>();

                try
                {
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);

                    myConnection.Open();

                    myCommand.Parameters.Add("@foodName", MySqlDbType.VarChar, 50).Value = foodItem.FoodItem.Name;
                    myCommand.Parameters.Add("@categoryId", MySqlDbType.Int32).Value = foodItem.FoodItem.FoodCategoryId;
                    myCommand.Parameters.Add("@quantity", MySqlDbType.Decimal).Value = foodItem.GroceryListFoodItem.Quantity;
                    myCommand.Parameters.Add("@listId", MySqlDbType.Int32).Value = foodItem.GroceryListFoodItem.ListId;
                    myCommand.Parameters.Add("@unitId", MySqlDbType.Int32).Value = foodItem.GroceryListFoodItem.UnitId;

                    errorMsg = "";

                    var rowsAffected = Convert.ToInt32(myCommand.ExecuteScalar());

                    if (rowsAffected == 0)
                    {
                        errorMsg = "Ingen matvara lades till.";
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