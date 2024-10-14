using System.ComponentModel.DataAnnotations;

namespace Bible.Models;

public class User
{
    [Key]
    [Required]
    [MaxLength(255)]
    public string Username { get; set; } = string.Empty;  
    [Required]
    [MaxLength(3072)]
    public string Collection { get; set; } = string.Empty;
}