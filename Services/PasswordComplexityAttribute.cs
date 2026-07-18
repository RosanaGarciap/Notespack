using System.ComponentModel.DataAnnotations;
using System.Linq;

public class PasswordComplexityAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        var var_PasswordValue = value as string ?? string.Empty;

        bool var_HasMinLength = var_PasswordValue.Length >= 12;
        bool var_HasUpper = var_PasswordValue.Any(char.IsUpper);
        bool var_HasLower = var_PasswordValue.Any(char.IsLower);
        bool var_HasDigit = var_PasswordValue.Any(char.IsDigit);
        bool var_HasSpecial = var_PasswordValue.Any(c => !char.IsLetterOrDigit(c));

        if (var_HasMinLength && var_HasUpper && var_HasLower && var_HasDigit && var_HasSpecial)
            return ValidationResult.Success;

        return new ValidationResult(
            "The password must be at least 12 characters long and include uppercase letters, lowercase letters, numbers, and a special character.");
    }
}