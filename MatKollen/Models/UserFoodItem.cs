using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MatKollen.Models
{
    public class UserFoodItem
    {
        public int Id { get; set; }
        public DateTime ExpirationDate { get; set; }
        public int UserId { get; set; } 
        public int FoodItemId { get; set; }

        public User User { get; set; }
        public FoodItem FoodItem { get; set; }
    }
}