using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MatKollen.Models
{
    public class ListFoodItem
    {
        public int Id { get; set; }
        public decimal Quantity { get; set; }
        public int UnitId { get; set; }
        public int ListId { get; set; } 
        public int FoodItemId { get; set; }
        public bool Completed { get; set; }
    }
}