using AutoCheckAML.Api.Entity.Base;

namespace AutoCheckAML.Api.Entity
{
    /// <summary>
    /// Representa un usuario del sistema.
    /// Soporta diferentes roles: DEV, SOFTWARE, INGENIERO, CUADRILLA
    /// </summary>
    public class User : AuditableEntity
    {
        /// <summary>
        /// Nombre de usuario único.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Email del usuario (único).
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Hash de la contraseña (nunca guardar en texto plano).
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// Nombre completo del usuario.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Indica si el usuario está activo en el sistema.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Última fecha de login del usuario.
        /// </summary>
        public DateTime? LastLogin { get; set; }

        /// <summary>
        /// ID de la cuadrilla a la que pertenece (si aplica).
        /// </summary>
        public int? CrewId { get; set; }

        // Navigation properties
        public virtual Crew Crew { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<FormSubmission> FormSubmissions { get; set; } = new List<FormSubmission>();
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
