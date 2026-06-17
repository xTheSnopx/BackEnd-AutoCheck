using System.Text.RegularExpressions;

namespace AutoCheckAML.Api.Helpers
{
    /// <summary>
    /// Clase de utilidad para desinfectar y validar entradas del usuario
    /// </summary>
    public static class InputSanitizer
    {
        /// <summary>
        /// Valida si una entrada contiene caracteres potencialmente peligrosos (XSS, inyecciones básicas)
        /// </summary>
        public static SanitizationResult ValidateInput(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return new SanitizationResult { IsValid = true, SanitizedValue = input };
            }

            // Detectar intentos comunes de inyección HTML/XSS o scripts
            bool hasThreat = Regex.IsMatch(input, @"[<>]|javascript:|onclick|onload|onerror|alert\(|eval\(", RegexOptions.IgnoreCase);

            return new SanitizationResult
            {
                IsValid = !hasThreat,
                SanitizedValue = input
            };
        }
    }

    public class SanitizationResult
    {
        public bool IsValid { get; set; }
        public string SanitizedValue { get; set; } = string.Empty;
    }
}
