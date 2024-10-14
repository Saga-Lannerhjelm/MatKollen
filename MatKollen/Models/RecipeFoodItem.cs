using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MatKollen.Models
{
    public class RecipeFoodItem
    {
        public int Id { get; set; }
        public int Amount { get; set; }
        public int UnitId { get; set; }
        public int RecipeId { get; set; } 
        public int FoodItemId { get; set; }
    }
}