using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatKollen.Models
{
    public class Recipe
{
    public int Id { get; set; }

    [Required]
    [StringLength(50, ErrorMessage = "Titel kan inte vara längre än 50 tecken.")]
    public string Title { get; set; }

    [Required]
    [StringLength(1000, ErrorMessage = "Instruktionerna kan inte vara längre än 1000 tecken.")]
    public required string Description { get; set; }

    public byte[]? ImageData { get; set; }
    public string? ImageContentType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Required]
    public int RecipeCategoryId { get; set; }

    [Required]
    public int UserId { get; set; }
}
}