using Microsoft.EntityFrameworkCore;
using NOTESPACK.Data;
using NOTESPACK.Models;
using BCrypt.Net; 

namespace NOTESPACK.Services
{
    public enum AuthLoginStatus
    {
        Success,
        EmailNotFound,
        WrongPassword
    }

    public class AuthLoginResult
    {
        public AuthLoginStatus Status { get; set; }
        public User? User { get; set; }
    }

    public class AuthService
    {
        private readonly IDbContextFactory<EventContext> _contextFactory;

        public AuthService(IDbContextFactory<EventContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<bool> RegisterAsync(string email, string password)
        {
            using var context = _contextFactory.CreateDbContext();
            if (await context.Users.AnyAsync(u => u.Email == email)) return false;

            var newUser = new User
            {
                Email = email,
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                Role = "User"
            };

            context.Users.Add(newUser);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<AuthLoginResult> LoginAsync(string email, string password)
        {
            using var context = _contextFactory.CreateDbContext();
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return new AuthLoginResult { Status = AuthLoginStatus.EmailNotFound };
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return new AuthLoginResult { Status = AuthLoginStatus.WrongPassword };
            }

            return new AuthLoginResult { Status = AuthLoginStatus.Success, User = user };
        }
        private async Task<string> Var_GenerateUniqueUsernameAsync(EventContext context)
        {
            string Var_Candidato;
            var Var_Intentos = 0;
            const int Var_MaxIntentos = 20;

            do
            {
                Var_Candidato = UsernameGenerator.Var_GenerateRandomUsername();
                Var_Intentos++;
            }
            while (await context.Users.AnyAsync(u => u.username == Var_Candidato) && Var_Intentos < Var_MaxIntentos);

            if (await context.Users.AnyAsync(u => u.username == Var_Candidato))
            {
                var Var_Sufijo = Guid.NewGuid().ToString("N")[..4];
                Var_Candidato = $"{Var_Candidato}_{Var_Sufijo}";
            }

            return Var_Candidato;
        }
    }

}