using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatKollen.Models
{
    public class FoodItem
    {
        public int Id { get; set; }

        [Required (ErrorMessage = "Fältet kan inte vara tomt")]                              
        [StringLength(20, ErrorMessage = "Nament på matvaran kan inte vara längre än 20 tecken")]
        public string? Name { get; set; }     

        [Required (ErrorMessage = "Fältet kan inte vara tomt")]
        [Range(1, int.MaxValue, ErrorMessage = "Du måste välja en kategori")]
        public int FoodCategoryId { get; set; }
    }
}