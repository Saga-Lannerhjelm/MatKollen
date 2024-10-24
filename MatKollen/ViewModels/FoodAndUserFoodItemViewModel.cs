using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MatKollen.Models;

namespace MatKollen.ViewModels
{
    public class FoodAndUserFoodItemViewModel
    {
        public required FoodItem FoodItem { get; set; }
        public required UserFoodItem UserFoodItem { get; set; }

    }
}