namespace AutoCheckAML.Api.Helpers
{
    /// <summary>
    /// Clase de utilidades para operaciones con strings
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        /// Normaliza un string (trim y lowercase)
        /// </summary>
        public static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLower();
        }

        /// <summary>
        /// Capitaliza la primera letra de un string
        /// </summary>
        public static string Capitalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return char.ToUpper(value[0]) + value.Substring(1).ToLower();
        }

        /// <summary>
        /// Trunca un string a una longitud máxima
        /// </summary>
        public static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
        }

        /// <summary>
        /// Remueve caracteres especiales de un string
        /// </summary>
        public static string RemoveSpecialCharacters(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return System.Text.RegularExpressions.Regex.Replace(value, "[^a-zA-Z0-9 ]", "");
        }
    }
}
