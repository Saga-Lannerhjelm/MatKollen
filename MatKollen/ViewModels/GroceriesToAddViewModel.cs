using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatKollen.Models;

namespace MatKollen.ViewModels
{
    public class GroceriesToAddViewModel
    {
        public required GroceryListFoodItem ListItem  { get; set; }
        public required string UnitType { get; set; }
        
    }
}