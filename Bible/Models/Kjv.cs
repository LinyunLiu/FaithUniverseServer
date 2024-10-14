using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bible.Models;

public class Kjv
{
    [Key]
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]  // Disable auto-increment
    public int Id { get; set; }
    [Required]
    [MaxLength(32)]
    public string BookName { get; set; } = string.Empty;
    [Required] 
    public int BookNumber { get; set; }
    [Required] 
    public int Chapter { get; set; }
    [Required] 
    public int Verse { get; set; }
    [Required]
    [MaxLength(1024)]
    public string Text { get; set; } = string.Empty;
    
    
  
}