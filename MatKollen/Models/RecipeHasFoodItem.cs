using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MatKollen.Models
{
    public class RecipeHasFoodItem
    {
        public int Id { get; set; }
        public int Antal { get; set; }
        public int RecipeId { get; set; } 
        public int FoodItemId { get; set; }

        public Recipe Recipte { get; set; }
        public FoodItem FoodItem { get; set; }
    }
}