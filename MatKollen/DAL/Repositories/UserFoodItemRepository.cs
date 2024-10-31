using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatKollen.Models;
using MatKollen.Services;
using MatKollen.ViewModels;
using MySql.Data.MySqlClient;

namespace MatKollen.DAL.Repositories
{
    public class UserFoodItemRepository
    {
         private readonly string _connectionString;
        private readonly ConvertQuantityHandler _convertQuantityHandler;
        
        public UserFoodItemRepository(IConfiguration configuration, ConvertQuantityHandler convertQuantityHandler)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");;
            _convertQuantityHandler = convertQuantityHandler;
        }

        public List<UserFoodItemViewModel>? GetUserFoodList(int userId, string searchPrompt, string category, string filter,  out string errorMsg)
        {
            var myConnectionString = _connectionString;
            string query  = "SELECT * FROM vw_user_food_details WHERE user_id = @userid";

            if (!string.IsNullOrEmpty(searchPrompt)) query += " AND item LIKE @searchPrompt";
            if (!string.IsNullOrEmpty(category) && category != "default") query += " AND category = @category";
            if (filter == "expiration_date")  query += " ORDER BY expiration_date";
            if (filter == "quantity")  query += " ORDER BY quantity";
            if (filter == "unit")  query += " ORDER BY unit";

            var foodItems = new List<UserFoodItemViewModel>();

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                try
                {
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);
                    myConnection.Open();
                    myCommand.Parameters.Add("@userId", MySqlDbType.Int32).Value = userId;
                    myCommand.Parameters.Add("@searchPrompt", MySqlDbType.VarChar, 50).Value = "%" + searchPrompt + "%";
                    myCommand.Parameters.Add("@category", MySqlDbType.VarChar, 50).Value = category;
                    myCommand.Parameters.Add("@filter", MySqlDbType.VarChar, 50).Value = filter;

                    errorMsg = "";

                    using var reader = myCommand.ExecuteReader();
                    {
                        while (reader.Read())
                        {
                            // Find if an item with the foodItemId and type already exists 
                            UserFoodItemViewModel? existingItem = null;
                            if (DateOnly.FromDateTime(reader.GetDateTime("expiration_date")) != new DateOnly())
                            {   
                                existingItem = foodItems.Find(item => item.FoodItemName.Contains(reader.GetString("item")) && item.UserFoodItems[0].UnitInfo.Type.Contains(reader.GetString("type")));
                            }

                            if (existingItem == null)
                            {
                                var foodItem = new UserFoodItemViewModel()
                                {
                                    FoodItemName = reader.GetString("item"),
                                    CategoryName = reader.GetString("category"),
                                    UserFoodItems =
                                    [
                                        new()
                                        {
                                            ConvertedQuantity = _convertQuantityHandler.ConvertFromLiterOrKg(reader.GetDecimal("quantity"), reader.GetDouble("conversion_multiplier")),
                                            FoodDetails = new UserFoodItem()
                                            {
                                                Id = reader.GetInt32("id"),
                                                Quantity = reader.GetDecimal("quantity"),
                                                UserId = reader.GetInt32("user_id"),
                                                ExpirationDate = DateOnly.FromDateTime(reader.GetDateTime("expiration_date")),
                                                FoodItemId = reader.GetInt16("food_item_id"),
                                                UnitId = reader.GetInt16("unit_id"),
                                            },
                                            UnitInfo = new MeasurementUnit() 
                                            {
                                                Unit = reader.GetString("unit"),
                                                Multiplier = reader.GetDouble("conversion_multiplier"),
                                                Type = reader.GetString("type")
                                            }
                                        } 
                                    ],
                                    SumOfQuantities = GetTotalQuantity(reader.GetInt16("food_item_id"), userId, reader.GetString("type"), out string error)
                                }; 
                                foodItems.Add(foodItem);
                            } else
                            {
                                existingItem?.UserFoodItems.Add(
                                    new()
                                    {
                                        ConvertedQuantity = _convertQuantityHandler.ConvertFromLiterOrKg(reader.GetDecimal("quantity"), reader.GetDouble("conversion_multiplier")),
                                        FoodDetails = new UserFoodItem()
                                        {
                                            Id = reader.GetInt32("id"),
                                            Quantity = reader.GetDecimal("quantity"),
                                            UserId = reader.GetInt32("user_id"),
                                            ExpirationDate = DateOnly.FromDateTime(reader.GetDateTime("expiration_date")),
                                            FoodItemId = reader.GetInt16("food_item_id"),
                                            UnitId = reader.GetInt16("unit_id"),
                                        },
                                        UnitInfo = new MeasurementUnit() 
                                        {
                                            Unit = reader.GetString("unit"),
                                            Multiplier = reader.GetDouble("conversion_multiplier"),
                                            Type = reader.GetString("type")
                                        }
                                    } 
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


        public decimal GetTotalQuantity(int foodItemId, int userId, string type, out string errorMsg)
        {
            var myConnectionString = _connectionString;

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                string query  = "SELECT SUM(quantity) AS sum FROM user_has_fooditems AS uhf " +
                                "INNER JOIN measurement_units AS ms ON ms.id = uhf.unit_id WHERE food_item_id = @foodItemId " +
                                "AND user_id = @userId AND ms.type = @type";
                decimal totalQuantity = 0;

                try
                {
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);
                    myConnection.Open();

                    myCommand.Parameters.Add("@foodItemId", MySqlDbType.Int32).Value = foodItemId;
                    myCommand.Parameters.Add("@userId", MySqlDbType.Int32).Value = userId;
                    myCommand.Parameters.Add("@type", MySqlDbType.VarChar, 50).Value = type;

                    errorMsg = "";

                    var reader = myCommand.ExecuteReader();

                    while (reader.Read())
                    {
                        totalQuantity = reader.GetDecimal("sum");
                    }
                    
                    return totalQuantity;
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

                    myCommand.Parameters.Add("@quantity", MySqlDbType.Decimal).Value = foodItem.Quantity;
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

        public int UpdateQuantity(int id, decimal nr, out string errorMsg)
        {
            var myConnectionString = _connectionString;

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                string query  = "UPDATE `user_has_fooditems` SET `quantity` = ROUND((`quantity` + @nr), 4) WHERE id = @id;";

                try
                {
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);
                    myConnection.Open();

                    myCommand.Parameters.Add("@nr", MySqlDbType.Decimal).Value = nr;
                    myCommand.Parameters.Add("@id", MySqlDbType.Int32).Value = id;

                    errorMsg = "";

                    var affectedRows = myCommand.ExecuteNonQuery();

                    if (affectedRows == 0)
                    {
                        errorMsg = "Det gick inte att uppdatera mängden av matvaran";
                        return 0;
                    }

                    return affectedRows;
                }
                catch (MySqlException e)
                {
                    errorMsg = e.Message;
                    return 0;
                }    
            }
        }

        public int UpdateExpirationDate(int id, DateOnly ExpirationDate, out string errorMsg)
        {
            var myConnectionString = _connectionString;

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                string query  = "UPDATE user_has_fooditems SET expiration_date = @expirationDate WHERE id = @id";

                try
                {
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);
                    myConnection.Open();

                    myCommand.Parameters.Add("@id", MySqlDbType.Int32).Value = id;
                    myCommand.Parameters.Add("@expirationDate", MySqlDbType.Date).Value = ExpirationDate;

                    errorMsg = "";

                    var rowsAffected = myCommand.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        errorMsg = "Gick inte att lägga till datumet datumet";
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

        public int DeleteAllOfFoodItem(int foodId, int userId, string type, out string errorMsg)
        {
            var myConnectionString = _connectionString;

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                string query  = "DELETE uhf FROM user_has_fooditems AS uhf INNER JOIN measurement_units AS ms ON ms.id = uhf.unit_id WHERE uhf.food_item_id = @foodId AND uhf.user_id = @userId AND ms.type = @type;";

                try
                {
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);
                    myConnection.Open();

                    myCommand.Parameters.Add("@foodId", MySqlDbType.Int32).Value = foodId;
                    myCommand.Parameters.Add("@userId", MySqlDbType.Int32).Value = userId;
                    myCommand.Parameters.Add("@type", MySqlDbType.VarChar, 50).Value = type;

                    errorMsg = "";

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

        public int DeleteFoodItem(int id, out string errorMsg)
        {
            var myConnectionString = _connectionString;

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                string query  = "DELETE FROM user_has_fooditems WHERE id = @id;";

                try
                {
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);
                    myConnection.Open();

                    myCommand.Parameters.Add("@id", MySqlDbType.Int32).Value = id;
                    errorMsg = "";

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