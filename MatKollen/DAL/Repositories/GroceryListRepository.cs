using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatKollen.Models;
using MatKollen.Services;
using MatKollen.ViewModels;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Bcpg;

namespace MatKollen.DAL.Repositories
{
    public class GroceryListRepository
    {
        private readonly string _connectionString;
        
        public GroceryListRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public int InsertOrUpdateFoodItems (ListFoodItem food, int userId, out string errorMsg)
        {
            if (GroceryListItemsExists(food.FoodItemId, userId, out errorMsg))
            {
                return UpdateFoodItemInList(food, out errorMsg);
            }
            else
            {
                return InsertFoodItemInList(food, userId, out errorMsg);
            }
        }

        public bool GroceryListItemsExists (int foodItemId, int userId, out string errorMsg)
        {
            var myConnectionString = _connectionString;
            string query  = "SELECT COUNT(*) FROM list_has_fooditems AS lhf INNER JOIN lists ON lists.id = lhf.list_id " +
                            "WHERE food_item_id = @foodItemId AND lists.user_id = @userId";
            var testList = new List<string>();

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                try
                {
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);
                    myConnection.Open();

                    myCommand.Parameters.Add("@foodItemId", MySqlDbType.Int32).Value = foodItemId;
                    myCommand.Parameters.Add("@userId", MySqlDbType.Int32).Value = userId;

                    errorMsg = "";

                    // execute the command and read the results
                    int exists = Convert.ToInt32(myCommand.ExecuteScalar());

                    if (exists > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (MySqlException e)
                {
                    errorMsg = e.Message;
                    return false;
                }    
            }
        }

        public int InsertFoodItemInList (ListFoodItem food, int userId, out string errorMsg)
        {
            var myConnectionString = _connectionString;

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                string query  = "INSERT INTO list_has_fooditems (quantity, unit_id, list_id, food_item_id) SELECT @quantity, @unitId, id, @foodItemId FROM lists WHERE user_id = @userId";
                var testList = new List<string>();

                try
                {
                    // create a MySQL command and set the SQL statement with parameters
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);

                    //open a connection
                    myConnection.Open();

                    myCommand.Parameters.Add("@userId", MySqlDbType.Int32).Value = userId;
                    myCommand.Parameters.Add("@quantity", MySqlDbType.Double).Value = food.Quantity;
                    myCommand.Parameters.Add("@unitId", MySqlDbType.Int32).Value = food.UnitId;
                    myCommand.Parameters.Add("@foodItemId", MySqlDbType.Int32).Value = food.FoodItemId;

                    errorMsg = "";

                    // execute the command and read the results
                    var rowsAffected = myCommand.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        errorMsg = "Gick inte att lägga till matvaran i inköpslistan";
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
        
        public int UpdateFoodItemInList (ListFoodItem food, out string errorMsg)
        {
            var myConnectionString = _connectionString;

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                string query  = "UPDATE list_has_fooditems SET quantity = (quantity + @addedQuantity) WHERE food_item_id = @foodItemId";
                var testList = new List<string>();

                try
                {
                    // create a MySQL command and set the SQL statement with parameters
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);

                    //open a connection
                    myConnection.Open();

                    myCommand.Parameters.Add("@addedQuantity", MySqlDbType.Double).Value = food.Quantity;
                    myCommand.Parameters.Add("@foodItemId", MySqlDbType.Int32).Value = food.FoodItemId;

                    errorMsg = "";

                    // execute the command and read the results
                    var rowsAffected = myCommand.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        errorMsg = "Gick inte att lägga till matvaran i inköpslistan";
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
        public List<GroceryListViewModel> GetGroceryList(int userId, out string errorMsg)
        {
            var myConnectionString = _connectionString;
            string query  = "SELECT * FROM vw_grocery_list_details WHERE `list_id` = (SELECT id FROM lists WHERE user_id = @userId)";
            
            var foodItems = new List<GroceryListViewModel>();
            var conversionHandler = new ConvertQuantityHandler();

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                try
                {
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);
                    myConnection.Open();

                    myCommand.Parameters.AddWithValue("@userId", userId);

                    errorMsg = "";

                    using var reader = myCommand.ExecuteReader();
                    {
                        while (reader.Read())
                        {
                            var foodItem = new GroceryListViewModel()
                            {
                                FoodDetails = new ListFoodItem()
                                {
                                    Id = reader.GetInt32("id"),
                                    Quantity = reader.GetDouble("quantity"),
                                    UnitId = reader.GetInt16("unit_id"),
                                    ListId = reader.GetInt16("list_id"),
                                    FoodItemId = reader.GetInt16("food_item_id"),
                                    Completed = reader.GetBoolean("completed")
                                },
                                ConvertedQuantity = conversionHandler.ConverFromtLiterOrKg(reader.GetDouble("quantity"), reader.GetDouble("conversion_multiplier")),
                                FoodItemName = reader.GetString("food_item"),
                                Unit = reader.GetString("unit"),
                                ListName = reader.GetString("list_name"),
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
    }
}