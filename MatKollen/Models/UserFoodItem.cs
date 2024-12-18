using System.ComponentModel.DataAnnotations;

namespace MatKollen.Models
{
    public class UserFoodItem
    {
        public int Id { get; set; }

        [Required (ErrorMessage = "Fältet kan inte vara tomt")]
        [Range(0.01, int.MaxValue, ErrorMessage = "Fältet måste vara mer än 0")]
        public decimal Quantity { get; set; }
        [Required (ErrorMessage = "Fältet kan inte vara tomt")]
        [DataType(DataType.ImageUrl, ErrorMessage = "Fältet måste vara ett giltigt datum")]
        public DateOnly ExpirationDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public int UserId { get; set; } 
        public int FoodItemId { get; set; }

        [Required (ErrorMessage = "Du måste välja en enhet")]
        [Range(1, int.MaxValue, ErrorMessage = "Välja en enhet")]
        public int UnitId { get; set; }
    }
}