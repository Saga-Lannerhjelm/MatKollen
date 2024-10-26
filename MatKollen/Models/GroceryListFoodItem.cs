using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MatKollen.Models
{
    public class GroceryListFoodItem
    {
        public int Id { get; set; }
        public decimal Quantity { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Du måste välja en enhet")]
        public int UnitId { get; set; }
        public int ListId { get; set; } 
        // [Range(1, int.MaxValue, ErrorMessage = "Du måste välja en vara")]
        public int FoodItemId { get; set; }
        public bool Completed { get; set; }
    }
}