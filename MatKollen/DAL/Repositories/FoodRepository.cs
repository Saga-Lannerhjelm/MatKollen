using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using MatKollen.Models;
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
                                        FoodItemId = reader.GetInt16("food_item_id"),
                                        UnitId = reader.GetInt16("unit_id"),
                                    },
                                    FoodItemName = reader.GetString("item"),
                                    CategoryName = reader.GetString("category"),
                                    ExpirationDate =
                                    [
                                        DateOnly.FromDateTime(reader.GetDateTime("expiration_date")),
                                    ],
                                    Amounts = 
                                    [
                                        reader.GetFloat("amount"),
                                    ],
                                    Units = 
                                    [
                                        reader.GetString("unit"),
                                    ],
                                }; 
                                foodItems.Add(foodItem);
                            } else
                            {
                                existingItem?.ExpirationDate?.Add(DateOnly.FromDateTime(reader.GetDateTime("expiration_date")));
                                existingItem?.Amounts?.Add(reader.GetFloat("amount"));
                                existingItem?.Units?.Add(reader.GetString("unit"));
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
    }  
}