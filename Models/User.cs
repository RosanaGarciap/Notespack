using System;
using System.ComponentModel.DataAnnotations;

namespace NOTESPACK.Models;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Email { get; set; } = string.Empty;

    // 👈 Agregamos 'username' para solucionar el error
    public string username { get; set; } = string.Empty; 

    // 👈 Adding "Password" instead of PasswordHash to make it consistent with files
    [Required]
    public string Password { get; set; } = string.Empty;

    // 👈 Adding  'EnrollmentDate' with the current date as default value
    public DateTime EnrollmentDate { get; set; } = DateTime.Now;

    public string Role { get; set; } = "User";

    public List<Event> Events { get; set; } = new();

    // Dentro de la clase User existente — solo agregar estas líneas nuevas:
    public bool EmailConfirmed { get; set; } = false;

}