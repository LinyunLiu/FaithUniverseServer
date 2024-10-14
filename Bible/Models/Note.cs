using System.ComponentModel.DataAnnotations;

namespace Bible.Models;

public class Note
{
    [Key]
    [Required]
    public int NoteId { get; set; }
    [Required]
    [MaxLength(255)]
    public string Username { get; set; } = string.Empty;
    [MaxLength(10000)] // Max 10,000 characters
    public string Content { get; set; } = string.Empty;
    [Required]
    public int ForVerseId { get; set; } 
}