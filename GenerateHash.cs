// Ejecutar con: dotnet script GenerateHash.cs
// O compilar y ejecutar

using System;

class Program
{
    static void Main()
    {
        // Usando BCrypt.Net
        Console.WriteLine("Admin2026: " + BCrypt.Net.BCrypt.HashPassword("Admin2026"));
        Console.WriteLine("Ingeniero2026: " + BCrypt.Net.BCrypt.HashPassword("Ingeniero2026"));
        Console.WriteLine("Supervisor2026: " + BCrypt.Net.BCrypt.HashPassword("Supervisor2026"));
        Console.WriteLine("Cuadrilla2026: " + BCrypt.Net.BCrypt.HashPassword("Cuadrilla2026"));
    }
}
