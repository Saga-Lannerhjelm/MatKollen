using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatKollen.Models;

namespace MatKollen.ViewModels
{
    public class RecipeFoodItemViewModel
    {
        public required RecipeFoodItem RecipeFood  { get; set; }
        public required string Ingredient { get; set; }
        public required string Unit { get; set; }
    }
}