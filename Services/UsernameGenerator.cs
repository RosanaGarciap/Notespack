namespace NOTESPACK.Services;

// Genera nombres de usuario aleatorios con el formato "ColorFruta_numero"
// (ej: "BlueMango_47") para asignarlos automáticamente al registrar una
// cuenta nueva, sin pedirle al usuario que elija uno.
public static class UsernameGenerator
{
    private static readonly string[] Var_Colors = new[]
    {
        "Red", "Blue", "Green", "Yellow", "Purple", "Pink", "Black",
        "White", "Gray", "Brown", "Teal", "Violet", "Gold", "Silver",
        "Crimson", "Emerald", "Turquoise", "Indigo", "Coral", "Amber"
    };

    private static readonly string[] Var_Fruits = new[]
    {
        "Apple", "Banana", "Mango", "Grape", "Melon", "Cherry", "Peach",
        "Plum", "Lemon", "Lime", "Kiwi", "Papaya", "Pear", "Fig",
        "Coconut", "Guava", "Pineapple", "Apricot", "Berry", "Watermelon"
    };

    // Returns an potential username. 
    // AuthService, que sí tiene acceso al contexto de la BD (ver Paso 2).
    public static string Var_GenerateRandomUsername()
    {
        var color = Var_Colors[Random.Shared.Next(Var_Colors.Length)];
        var fruta = Var_Fruits[Random.Shared.Next(Var_Fruits.Length)];
        var numero = Random.Shared.Next(1, 101); // 101 excluido => rango real 1-100

        return $"{color}{fruta}_{numero}";
    }
}