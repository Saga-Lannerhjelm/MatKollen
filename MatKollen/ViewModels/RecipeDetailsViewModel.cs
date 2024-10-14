using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatKollen.Models;

namespace MatKollen.ViewModels
{
    public class RecipeDetailsViewModel
    {
        public Recipe Recipe { get; set; }
        public string Category { get; set; }
        public string Username { get; set; }
        public List<RecipeFoodItemViewModel> Ingredients { get; set; } = new List<RecipeFoodItemViewModel>();
    }
}