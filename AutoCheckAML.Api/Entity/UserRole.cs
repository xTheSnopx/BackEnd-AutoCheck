namespace AutoCheckAML.Api.Entity
{
    /// <summary>
    /// Tabla de unión (Many-to-Many) entre Usuario y Rol.
    /// Permite que un usuario tenga múltiples roles.
    /// </summary>
    public class UserRole
    {
        /// <summary>
        /// ID del usuario.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// ID del rol.
        /// </summary>
        public int RoleId { get; set; }

        /// <summary>
        /// Fecha de asignación del rol al usuario.
        /// </summary>
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de vencimiento del rol (null = sin vencimiento).
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Indicador de si la asignación está activa.
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual User User { get; set; }
        public virtual Role Role { get; set; }
    }
}
