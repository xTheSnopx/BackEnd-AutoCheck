using AutoCheckAML.Api.Entity.Base;

namespace AutoCheckAML.Api.Entity
{
    /// <summary>
    /// Representa un permiso específico en el sistema.
    /// Ejemplo: CreateUser, DeleteForm, ExportToExcel, ViewAuditLog
    /// </summary>
    public class Permission : AuditableEntity
    {
        /// <summary>
        /// Nombre único del permiso (ej: CREATE_USER, VIEW_DASHBOARD).
        /// Formato: ACCION_RECURSO (preferiblemente en UPPER_CASE)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descripción del permiso.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Categoría del permiso para organización (ej: User Management, Forms, Exports, Audit).
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Indica si el permiso es crítico para el sistema.
        /// </summary>
        public bool IsCritical { get; set; } = false;

        /// <summary>
        /// Indica si el permiso está activo.
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
