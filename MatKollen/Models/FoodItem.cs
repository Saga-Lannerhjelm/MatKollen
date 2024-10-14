using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatKollen.Models
{
    public class FoodItem
    {
        public int Id { get; set; }

        [Required]                              
        [StringLength(20, ErrorMessage = "Nament på matvaran kan inte vara längre än 20 tecken.")]
        public required string Name { get; set; }     

        [Required]
        public int FoodCategoryId { get; set; }
    }
}