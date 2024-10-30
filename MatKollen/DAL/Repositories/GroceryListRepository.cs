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
        private readonly ConvertQuantityHandler _convertQuantityHandler;
        
        public GroceryListRepository(IConfiguration configuration, ConvertQuantityHandler convertQuantityHandler)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _convertQuantityHandler = convertQuantityHandler;
        }

        public List<GroceryListViewModel> GetGroceryList(int userId, out string errorMsg)
        {
            var myConnectionString = _connectionString;
            string query  = "SELECT * FROM vw_grocery_list_details WHERE `list_id` = (SELECT id FROM lists WHERE user_id = @userId)";
            
            var foodItems = new List<GroceryListViewModel>();

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
                                FoodDetails = new GroceryListFoodItem()
                                {
                                    Id = reader.GetInt32("id"),
                                    Quantity = reader.GetDecimal("quantity"),
                                    UnitId = reader.GetInt16("unit_id"),
                                    ListId = reader.GetInt16("list_id"),
                                    FoodItemId = reader.GetInt16("food_item_id"),
                                    Completed = reader.GetBoolean("completed")
                                },
                                ConvertedQuantity = _convertQuantityHandler.ConvertFromLiterOrKg(reader.GetDecimal("quantity"), reader.GetDouble("conversion_multiplier")),
                                FoodItemName = reader.GetString("food_item"),
                                Unit = reader.GetString("unit"),
                                ListName = reader.GetString("list_name"),
                            }; 
                            foodItems.Add(foodItem);
                        }
                        // ...
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

        public List<GroceryListFoodItem> GetCompletedItems(int userId, out string errorMsg)
        {
            var myConnectionString = _connectionString;
            string query  = "SELECT * FROM vw_grocery_list_details WHERE `list_id` = (SELECT id FROM lists WHERE user_id = @userId) AND completed = true";
            
            var foodItems = new List<GroceryListFoodItem>();

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
                            var foodItem = new GroceryListFoodItem()
                            {
                                Id = reader.GetInt32("id"),
                                Quantity = reader.GetDecimal("quantity"),
                                UnitId = reader.GetInt16("unit_id"),
                                ListId = reader.GetInt16("list_id"),
                                FoodItemId = reader.GetInt16("food_item_id"),
                                Completed = reader.GetBoolean("completed")
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

        public int GetGroceryListId(int userId, out string errorMsg)
        {
            var myConnectionString = _connectionString;
            string query  = "SELECT id FROM lists WHERE user_id = @userId";
            
            var listId = 0;

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
                            listId = reader.GetInt32("id");
                        }
                    }
                    return listId;
                }
                catch (MySqlException e)
                {
                    errorMsg = e.Message;
                    return 0;
                }
            }
        }

        public bool GroceryListItemsExists (int foodItemId, string type, int userId, out string errorMsg)
        {
            var myConnectionString = _connectionString;
            string query = "SELECT COUNT(*) FROM list_has_fooditems AS lhf " + 
                           "INNER JOIN lists ON lists.id = lhf.list_id " + 
                           "INNER JOIN measurement_units AS ms ON ms.id = lhf.unit_id " +
                           "WHERE food_item_id = @foodItemId AND lists.user_id = @userId AND ms.type = @type";

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                try
                {
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);
                    myConnection.Open();

                    myCommand.Parameters.Add("@foodItemId", MySqlDbType.Int32).Value = foodItemId;
                    myCommand.Parameters.Add("@userId", MySqlDbType.Int32).Value = userId;
                    myCommand.Parameters.Add("@type", MySqlDbType.VarChar, 50).Value = type;

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

        public int InsertOrUpdateFoodItems (GroceriesToAddViewModel food, int userId, out string errorMsg)
        {
            if (GroceryListItemsExists(food.ListItem.FoodItemId, food.UnitType, userId, out errorMsg))
            {
                return UpdateFoodItemInList(food.ListItem, food.UnitType, out errorMsg);
            }
            else
            {
                return InsertFoodItemInList(food.ListItem, userId, out errorMsg);
            }
        }

        public int InsertFoodItemInList (GroceryListFoodItem food, int userId, out string errorMsg)
        {
            var myConnectionString = _connectionString;

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                string query  = "INSERT INTO list_has_fooditems (quantity, unit_id, list_id, food_item_id) SELECT @quantity, @unitId, id, @foodItemId FROM lists WHERE user_id = @userId";

                try
                {
                    // create a MySQL command and set the SQL statement with parameters
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);

                    //open a connection
                    myConnection.Open();

                    myCommand.Parameters.Add("@userId", MySqlDbType.Int32).Value = userId;
                    myCommand.Parameters.Add("@quantity", MySqlDbType.Decimal).Value = food.Quantity;
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
        
        public int UpdateFoodItemInList (GroceryListFoodItem food, string type, out string errorMsg)
        {
            var myConnectionString = _connectionString;

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                string query  = "UPDATE list_has_fooditems lhf INNER JOIN measurement_units AS ms ON lhf.unit_id = ms.id SET quantity = (quantity + @addedQuantity) " +
                                "WHERE food_item_id = @foodItemId AND ms.type = @type";

                try
                {
                    // create a MySQL command and set the SQL statement with parameters
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);

                    //open a connection
                    myConnection.Open();

                    myCommand.Parameters.Add("@addedQuantity", MySqlDbType.Decimal).Value = food.Quantity;
                    myCommand.Parameters.Add("@foodItemId", MySqlDbType.Int32).Value = food.FoodItemId;
                    myCommand.Parameters.Add("@type", MySqlDbType.VarChar, 50).Value = type;

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

        public bool UpdateCompletedState (int id, out string errorMsg)
        {
            var myConnectionString = _connectionString;

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                string query  = "CALL sp_update_completed_state(@id)";
                bool isCompleted = false;

                try
                {
                    // create a MySQL command and set the SQL statement with parameters
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);

                    //open a connection
                    myConnection.Open();

                    myCommand.Parameters.Add("@id", MySqlDbType.Int32).Value = id;;

                    errorMsg = "";

                    // execute the command and read the results
                    var reader = myCommand.ExecuteReader();

                    while (reader.Read())
                    {
                        isCompleted = reader.GetBoolean("completed");
                    }
                    
                    return isCompleted;
                }
                catch (MySqlException e)
                {
                    errorMsg = e.Message;
                    return false;
                }    
            }
        }
    
        public int Delete(int id, out string errorMsg)
        {
            var myConnectionString = _connectionString;

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                string query  = "DELETE FROM list_has_fooditems WHERE id = @id";

                try
                {
                    // create a MySQL command and set the SQL statement with parameters
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);

                    //open a connection
                    myConnection.Open();

                    myCommand.Parameters.Add("@id", MySqlDbType.Double).Value = id;

                    errorMsg = "";

                    // execute the command and read the results
                    var rowsAffected = myCommand.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        errorMsg = "Gick inte att ta bort";
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