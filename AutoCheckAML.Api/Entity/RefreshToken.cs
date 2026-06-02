namespace AutoCheckAML.Api.Entity
{
    /// <summary>
    /// Almacena los tokens de refresco (refresh tokens) para mantener sesiones activas.
    /// </summary>
    public class RefreshToken
    {
        /// <summary>
        /// ID único del refresh token.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID del usuario propietario del token.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Token de refresco cifrado.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Fecha de expiración del refresh token.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Dirección IP desde donde se emitió el token.
        /// </summary>
        public string IssuedFromIp { get; set; }

        /// <summary>
        /// Fecha de creación del token.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de revocación (si ha sido revocado).
        /// </summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// Indica si el token ha sido utilizado.
        /// </summary>
        public bool IsUsed { get; set; } = false;

        /// <summary>
        /// Indica si el token ha sido revocado.
        /// </summary>
        public bool IsRevoked { get; set; } = false;

        /// <summary>
        /// Indica si el token ha expirado (calculado).
        /// </summary>
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        // Navigation properties
        public virtual User User { get; set; }
    }
}
