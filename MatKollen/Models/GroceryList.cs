using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatKollen.Models
{
    public class GroceryList
    {
        public int Id { get; set; }
                        
        public string? Name { get; set; }     

        public int UserId { get; set; }
    }
}