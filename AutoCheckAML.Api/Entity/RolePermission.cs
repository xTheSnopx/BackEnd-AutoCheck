namespace AutoCheckAML.Api.Entity
{
    /// <summary>
    /// Tabla de unión (Many-to-Many) entre Rol y Permiso.
    /// Define qué permisos tiene cada rol.
    /// </summary>
    public class RolePermission
    {
        /// <summary>
        /// ID del rol.
        /// </summary>
        public int RoleId { get; set; }

        /// <summary>
        /// ID del permiso.
        /// </summary>
        public int PermissionId { get; set; }

        /// <summary>
        /// Fecha de asignación del permiso al rol.
        /// </summary>
        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Indica si la asignación está activa.
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual Role Role { get; set; }
        public virtual Permission Permission { get; set; }
    }
}
