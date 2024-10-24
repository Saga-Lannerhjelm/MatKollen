using System.ComponentModel.DataAnnotations;

namespace MatKollen.Models
{
    public class RecipeCategory
    {
        public int Id { get; set; }
                           
        [StringLength(20, ErrorMessage = "Nament på kategorin kan inte vara längre än 20 tecken.")]
        public required string Name { get; set; }

    }
}