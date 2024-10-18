using System.ComponentModel.DataAnnotations;

namespace MatKollen.Models
{
    public class FoodCategory
    {
        public int Id { get; set; }                          
        public required string Name { get; set; } 
    }
}