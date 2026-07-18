using System.ComponentModel.DataAnnotations;

namespace NOTESPACK.Models
{
    public class EmailVerificationToken
    {
        [Key]
        public int Id { get; set; } // sin prefijo: convención de EF Core para clave primaria

        public int Var_UserId { get; set; }
        public User User { get; set; } = null!; // reutiliza User.cs existente, no se renombra

        public string Var_TokenHash { get; set; } = string.Empty;
        public DateTime Var_ExpiresAtUtc { get; set; }
        public bool Var_IsUsed { get; set; } = false;
        public DateTime Var_CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}