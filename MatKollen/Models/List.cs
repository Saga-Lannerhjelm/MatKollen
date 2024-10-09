using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatKollen.Models
{
    public class List
    {
        [Key]
        public int Id { get; set; }

        [Required]                              
        [StringLength(20, ErrorMessage = "Nament på listan kan inte vara längre än 20 tecken.")]
        public string Name { get; set; }     

        [ForeignKey("User")]
        [Required]
        public int UserId { get; set; }
    }
}