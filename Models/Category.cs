using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Notespack.Models;

public class Category
{
    [Key]
    [Range(1, uint.MaxValue, ErrorMessage = "ID must be a positive number greater than 0.")]
    [Required]
    public int Id { get; set; }

    [RegularExpression(@"^[A-Z]+[a-zA-Z\s]*$")]
    [Required]
    [StringLength(30, ErrorMessage = "The Name can't have more than 30 characters.")]
    public string? Name { get; set; }

}