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
                                    FoodItemDetails = new UserFoodItem()
                                    {
                                        Quantity = reader.GetDouble("quantity"),
                                        FoodItemId = reader.GetInt16("food_item_id"),
                                        UnitId = reader.GetInt16("unit_id"),
                                    },
                                    FoodItemName = reader.GetString("item"),
                                    CategoryName = reader.GetString("category"),
                                    ExpirationDate =
                                    [
                                        DateOnly.FromDateTime(reader.GetDateTime("expiration_date")),
                                    ],
                                    Quantities =
                                    [
                                        (ConvertedQuantity: conversionHandler.ConverFromtLiterOrKg(reader.GetDouble("quantity"), reader.GetDouble("conversion_multiplier")),
                                        Convert: reader.GetDouble("quantity"))
                                    ],
                                    Units = 
                                    [
                                        (Unit: reader.GetString("unit"),
                                        Type: reader.GetString("type"))
                                    ],
                                }; 
                                foodItems.Add(foodItem);
                            } else
                            {
                                existingItem?.ExpirationDate?.Add(DateOnly.FromDateTime(reader.GetDateTime("expiration_date")));
                                existingItem?.Quantities?.Add((ConvertedQuantity: conversionHandler.ConverFromtLiterOrKg(reader.GetDouble("quantity"), reader.GetDouble("conversion_multiplier")), Convert: reader.GetDouble("quantity")));
                                existingItem?.Units?.Add((Unit: reader.GetString("unit"), Type: reader.GetString("type")));
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
    }  
}