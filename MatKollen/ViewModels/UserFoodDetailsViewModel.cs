using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatKollen.Models;

namespace MatKollen.ViewModels
{
    public class UserFoodDetailsViewModel
    {
        public decimal ConvertedQuantity {set; get;}
        public required UserFoodItem FoodDetails {set; get;}
        public required MeasurementUnit UnitInfo {set; get;}
    }
}