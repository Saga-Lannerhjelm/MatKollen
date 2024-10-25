using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace MatKollen.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required (ErrorMessage = "Fältet kan inte vara tomt")]              
        [StringLength(20, ErrorMessage = "Användarnamnet kan inte vara längre än 20 tecken.")]
        public string? Username { get; set; }    
                               
        [StringLength(320, ErrorMessage = "Email kan inte vara längre än 320 tecken.")]
        [EmailAddress(ErrorMessage = "Ogiltig emailadress")]
        public string? Email { get; set; }   

        [Required (ErrorMessage = "Fältet kan inte vara tomt")]
        public string? Password { get; set; }  

        public string? Salt { get; set;}
    }
}