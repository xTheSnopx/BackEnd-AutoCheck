namespace AutoCheckAML.Api.Helpers
{
    /// <summary>
    /// Clase de utilidades para validaciones comunes
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Valida si un string es un email válido
        /// </summary>
        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Valida si un string no es nulo ni vacío
        /// </summary>
        public static bool IsNotEmpty(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Valida si un teléfono tiene un formato válido (mínimo 7 dígitos)
        /// </summary>
        public static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            var digitsOnly = new string(phone.Where(char.IsDigit).ToArray());
            return digitsOnly.Length >= 7;
        }
    }
}
