using System.ComponentModel.DataAnnotations;


namespace MatKollen.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]                              
        [StringLength(20, ErrorMessage = "Användarnamnet kan inte vara längre än 20 tecken.")]
        public string Username { get; set; }    
                               
        [StringLength(320, ErrorMessage = "Email kan inte vara längre än 320 tecken.")]
        [EmailAddress(ErrorMessage = "Ogiltig emailadress")]
        public string? Email { get; set; }   

        [Required]   
        public byte[] Password { get; set; }  

         public ICollection<List> Lists {get; set;}
         public ICollection<Recipe> Recipes {get; set;}
    }
}