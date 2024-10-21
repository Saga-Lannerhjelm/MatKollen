using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MatKollen.Models
{
    public class RecipeFoodItem
    {
        public int Id { get; set; }

        [Required (ErrorMessage = "Fältet kan inte vara tomt")]
        [Range(0.1, int.MaxValue, ErrorMessage = "Välj ett högre värde, och/eller byt enhet")]
        public double Quantity { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Du måste välja en enhet")]
        public int UnitId { get; set; }
        public int RecipeId { get; set; } 

        [Range(1, int.MaxValue, ErrorMessage = "Du måste välja en vara")]
        public int FoodItemId { get; set; }
    }
}