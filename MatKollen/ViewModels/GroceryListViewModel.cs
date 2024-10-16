using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatKollen.Models;

namespace MatKollen.ViewModels
{
    public class GroceryListViewModel
    {
        public required ListFoodItem FoodDetails  { get; set; }

        public double ConvertedQuantity { get; set; }
        public required string FoodItemName { get; set; }
        public required string Unit { get; set; }
        public required string ListName { get; set; }
    }
}