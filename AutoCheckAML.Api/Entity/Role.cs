using AutoCheckAML.Api.Entity.Base;

namespace AutoCheckAML.Api.Entity
{
    /// <summary>
    /// Representa un rol en el sistema (DEV, SOFTWARE, INGENIERO, CUADRILLA).
    /// </summary>
    public class Role : AuditableEntity
    {
        /// <summary>
        /// Nombre único del rol.
        /// Valores posibles: DEV, SOFTWARE, INGENIERO_MECANICO, SUPERVISOR_HSEQ, CUADRILLA
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descripción del rol y sus responsabilidades.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Nivel jerárquico (0=DEV, 1=SOFTWARE, 2=INGENIERO, 3=CUADRILLA).
        /// </summary>
        public int HierarchyLevel { get; set; }

        /// <summary>
        /// Indica si el rol es un rol de sistema (no se puede eliminar).
        /// </summary>
        public bool IsSystemRole { get; set; } = false;

        /// <summary>
        /// Indica si el rol está activo en el sistema.
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
