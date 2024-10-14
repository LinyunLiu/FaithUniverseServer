using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bible.Models;

public class Account
{
    [Key]      
    [Required]
    [MaxLength(255)]
    public string Id { get; set; } = Guid.NewGuid().ToString(); // Automatically generated  
    [Required]
    [MaxLength(255)]
    public string Username { get; set; } = string.Empty;    
    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;    
    [Required]
    [MaxLength(255)]
    public string Password { get; set; } = string.Empty;
    
    [NotMapped]
    [MaxLength(255)]   
    public string PermissionKey { get; set; } = string.Empty; // Excluded from mapping to database
}