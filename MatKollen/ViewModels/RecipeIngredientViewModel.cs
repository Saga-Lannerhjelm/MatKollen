using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatKollen.Models;

namespace MatKollen.ViewModels
{
    public class RecipeIngredientViewModel
    {
        public required RecipeFoodItem IngredientDetails  { get; set; }
        public required string Ingredient { get; set; }
        public required string Unit { get; set; }
        public double ConvertedQuantity { get; set; }
        public bool UserHasIngredient { get; set; } = false;
    }
}