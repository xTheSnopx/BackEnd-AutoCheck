namespace AutoCheckAML.Api.Entity.Base
{
    /// <summary>
    /// Entidad con capacidad de auditoría y soft-delete.
    /// Hereda de BaseEntity y agrega campos para rastrear cambios.
    /// </summary>
    public abstract class AuditableEntity : BaseEntity
    {
        /// <summary>
        /// Fecha y hora de última actualización en UTC.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Fecha y hora de eliminación lógica (soft-delete) en UTC.
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Indica si el registro ha sido eliminado lógicamente.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// ID del usuario que realizó la última modificación.
        /// </summary>
        public int? LastModifiedBy { get; set; }

        /// <summary>
        /// ID del usuario que eliminó el registro (si aplica).
        /// </summary>
        public int? DeletedBy { get; set; }

        /// <summary>
        /// Marca la entidad como actualizada con timestamp actual.
        /// </summary>
        public void MarkAsUpdated()
        {
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Realiza un soft-delete de la entidad.
        /// </summary>
        /// <param name="deletedByUserId">ID del usuario que realiza la eliminación.</param>
        public void SoftDelete(int deletedByUserId)
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            DeletedBy = deletedByUserId;
        }

        /// <summary>
        /// Restaura una entidad eliminada (undo soft-delete).
        /// </summary>
        public void Restore()
        {
            IsDeleted = false;
            DeletedAt = null;
            DeletedBy = null;
        }
    }
}
