using System;
using System.Collections.Generic;
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
        public List<UserFoodItemViewModel>? GetUserFoodList(int userId, out string errorMsg)
        {
            var myConnectionString = "Server=localhost;Database=MatKollen;Uid=root;Pwd=mySqlPw123;";

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                string query  = "SELECT * FROM vw_user_food_detials WHERE user_id = @userid";
                var foodItems = new List<UserFoodItemViewModel>();

                try
                {
                    // create a MySQL command and set the SQL statement with parameters
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);
                    myConnection.Open();

                    myCommand.Parameters.AddWithValue("@userId", userId);

                    errorMsg = "";

                    // execute the command and read the results
                    using var reader = myCommand.ExecuteReader();
                    {
                        while (reader.Read())
                        {
                            if (foodItems.Find(item => item.FoodItem.Contains(reader.GetString("item"))) == null)
                            {
                                var foodItem = new UserFoodItemViewModel()
                                {
                                    FoodItem = reader.GetString("item"),
                                    Category = reader.GetString("category"),
                                    ExpirationDate =
                                    [
                                        DateOnly.FromDateTime(reader.GetDateTime("expiration_date")),
                                    ]
                                }; 
                                foodItems.Add(foodItem);
                            } else
                            {
                                var existingItem = foodItems.Find(item => item.FoodItem.Contains(reader.GetString("item")));
                                existingItem?.ExpirationDate?.Add(DateOnly.FromDateTime(reader.GetDateTime("expiration_date")));
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