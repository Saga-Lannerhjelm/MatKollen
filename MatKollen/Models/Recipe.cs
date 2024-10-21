using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatKollen.Models
{
    public class Recipe
{
    public int Id { get; set; }

    [Required (ErrorMessage = "Fältet kan inte vara tomt")]
    [StringLength(50, ErrorMessage = "Titel kan inte vara längre än 50 tecken.")]
    public string? Title { get; set; }

    [Required (ErrorMessage = "Fältet kan inte vara tomt")]
    [StringLength(1000, ErrorMessage = "Instruktionerna kan inte vara längre än 1000 tecken.")]
    public string? Description { get; set; }
    public byte[]? ImageData { get; set; }
    public string? ImageContentType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Required (ErrorMessage = "Fältet kan inte vara tomt")]
    [Range(1, int.MaxValue, ErrorMessage = "Du måste välja en kategori")]
    public int RecipeCategoryId { get; set; }
    public int UserId { get; set; }
}
}