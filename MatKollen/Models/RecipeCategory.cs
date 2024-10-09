using System.ComponentModel.DataAnnotations;

namespace MatKollen.Models
{
    public class RecipeCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]                              
        [StringLength(20, ErrorMessage = "Nament på kategorin kan inte vara längre än 20 tecken.")]
        public string Name { get; set; }

         public ICollection<Recipe> Recipes {get; set;}

    }
}