using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MatKollen.Models
{
    public class ListHasFoodItem
    {
         public int Id { get; set; }
        public int Antal { get; set; }
        public int ListId { get; set; } 
        public int FoodItemId { get; set; }

        public List List { get; set; }
        public FoodItem FoodItem { get; set; }
    }
}