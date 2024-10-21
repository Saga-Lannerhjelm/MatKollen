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
        
        public FoodRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public List<UserFoodItemViewModel>? GetUserFoodList(int userId, out string errorMsg)
        {
            var myConnectionString = _connectionString;
            string query  = "SELECT * FROM vw_user_food_details WHERE user_id = @userid";
            var foodItems = new List<UserFoodItemViewModel>();
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
                            var existingItem = foodItems.Find(item => item.FoodItemName.Contains(reader.GetString("item")));

                            if (existingItem == null)
                            {
                                var foodItem = new UserFoodItemViewModel()
                                {
                                    FoodItemName = reader.GetString("item"),
                                    CategoryName = reader.GetString("category"),
                                    UserFoodItems = 
                                    [
                                        (
                                            ConvertedQuantity: conversionHandler.ConverFromtLiterOrKg(reader.GetDouble("quantity"), reader.GetDouble("conversion_multiplier")),
                                            FoodDetails: new UserFoodItem()
                                            {
                                                Id = reader.GetInt32("id"),
                                                Quantity = reader.GetDouble("quantity"),
                                                UserId = reader.GetInt32("user_id"),
                                                ExpirationDate = DateOnly.FromDateTime(reader.GetDateTime("expiration_date")),
                                                FoodItemId = reader.GetInt16("food_item_id"),
                                                UnitId = reader.GetInt16("unit_id"),
                                            },
                                            UnitInfo: new MeasurementUnit() 
                                            {
                                                Unit = reader.GetString("unit"),
                                                Multiplier = reader.GetDouble("conversion_multiplier"),
                                                Type = reader.GetString("type")
                                            }
                                        )
                                    ],
                                }; 
                                foodItems.Add(foodItem);
                            } else
                            {
                                existingItem?.UserFoodItems.Add(
                                    (
                                            ConvertedQuantity: conversionHandler.ConverFromtLiterOrKg(reader.GetDouble("quantity"), reader.GetDouble("conversion_multiplier")),
                                            FoodDetails: new UserFoodItem()
                                            {
                                                Id = reader.GetInt32("id"),
                                                Quantity = reader.GetDouble("quantity"),
                                                UserId = reader.GetInt32("user_id"),
                                                ExpirationDate = DateOnly.FromDateTime(reader.GetDateTime("expiration_date")),
                                                FoodItemId = reader.GetInt16("food_item_id"),
                                                UnitId = reader.GetInt16("unit_id"),
                                            },
                                            UnitInfo: new MeasurementUnit() 
                                            {
                                                Unit = reader.GetString("unit"),
                                                Multiplier = reader.GetDouble("conversion_multiplier"),
                                                Type = reader.GetString("type")
                                            }
                                        )
                                );
                            }
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

        public int InsertFoodItem(AddFoodAndUserItemViewModel foodItem, out string errorMsg)
        {
            var myConnectionString = _connectionString;

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                //Creates a new user and a grozery list for that user at the same time
                string query  = "CALL `insert_food_and_assign_to_user`(@foodName, @categoryId, @quantity, @expirationDate, @userId, @unitId);";
                var testList = new List<string>();

                try
                {
                    // create a MySQL command and set the SQL statement with parameters
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);

                    //open a connection
                    myConnection.Open();

                    myCommand.Parameters.Add("@foodName", MySqlDbType.VarChar, 50).Value = foodItem.FoodItem.Name;
                    myCommand.Parameters.Add("@categoryId", MySqlDbType.Int32).Value = foodItem.FoodItem.FoodCategoryId;
                    myCommand.Parameters.Add("@quantity", MySqlDbType.Double).Value = foodItem.UserFoodItem.Quantity;
                    myCommand.Parameters.Add("@expirationDate", MySqlDbType.Date).Value = foodItem.UserFoodItem.ExpirationDate;
                    myCommand.Parameters.Add("@userId", MySqlDbType.Int32).Value = foodItem.UserFoodItem.UserId;
                    myCommand.Parameters.Add("@unitId", MySqlDbType.Int32).Value = foodItem.UserFoodItem.UnitId;

                    errorMsg = "";

                    // execute the command and read the results
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

        public int AddFoodItem(UserFoodItem foodItem, out string errorMsg)
        {
            var myConnectionString = _connectionString;

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                string query  = "Call add_food_item(@quantity, @expirationDate, @userId, @foodItemId, @unitId)";
                var testList = new List<string>();

                try
                {
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);
                    myConnection.Open();

                    myCommand.Parameters.Add("@quantity", MySqlDbType.Double).Value = foodItem.Quantity;
                    myCommand.Parameters.Add("@expirationDate", MySqlDbType.Date).Value = foodItem.ExpirationDate;
                    myCommand.Parameters.Add("@userId", MySqlDbType.Int32).Value = foodItem.UserId;
                    myCommand.Parameters.Add("@foodItemId", MySqlDbType.Int32).Value = foodItem.FoodItemId;
                    myCommand.Parameters.Add("@unitId", MySqlDbType.Int32).Value = foodItem.UnitId;

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

        public int UpdateQuantity(int id, double nr, out string errorMsg)
        {
            var myConnectionString = _connectionString;

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                string query  = "Update user_has_fooditems SET quantity = ROUND((quantity + @nr), 4) WHERE id = @id";

                try
                {
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);
                    myConnection.Open();

                    myCommand.Parameters.Add("@nr", MySqlDbType.Double).Value = nr;
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

        public int Delete(int foodId, int userId, string type, out string errorMsg)
        {
            var myConnectionString = _connectionString;

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                //Creates a new user and a grozery list for that user at the same time
                string query  = "DELETE uhf FROM user_has_fooditems AS uhf INNER JOIN measurement_units AS ms ON ms.id = uhf.unit_id WHERE uhf.food_item_id = @foodId AND uhf.user_id = @userId AND ms.type = @type;";

                try
                {
                    // create a MySQL command and set the SQL statement with parameters
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);

                    //open a connection
                    myConnection.Open();

                    myCommand.Parameters.Add("@foodId", MySqlDbType.Int32).Value = foodId;
                    myCommand.Parameters.Add("@userId", MySqlDbType.Int32).Value = userId;
                    myCommand.Parameters.Add("@type", MySqlDbType.VarChar, 50).Value = type;

                    errorMsg = "";

                    // execute the command and read the results
                    var rowsAffected = myCommand.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        errorMsg = "Ingen matvara togs bort";
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