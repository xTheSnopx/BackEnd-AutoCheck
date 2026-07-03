namespace AutoCheckAML.Api.Entity.Base
{
    /// <summary>
    /// Entidad para modelos con estados/status (flujos de trabajo).
    /// Hereda de AuditableEntity y agrega campos de estado.
    /// Ejemplo: Formularios con estados Pendiente → Revisado → Completado
    /// </summary>
    public abstract class StatusEntity : AuditableEntity
    {
        /// <summary>
        /// Estado actual del registro.
        /// Los estados posibles dependen de cada implementación.
        /// </summary>
        public string Status { get; set; } = "Pendiente";

        /// <summary>
        /// Comentario o notas sobre el cambio de estado.
        /// </summary>
        public string StatusComment { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de la última transición de estado.
        /// </summary>
        public DateTime? LastStatusChangeAt { get; set; }

        /// <summary>
        /// ID del usuario que realizó el último cambio de estado.
        /// </summary>
        public int? LastStatusChangedBy { get; set; }

        /// <summary>
        /// Actualiza el estado de la entidad con auditoría.
        /// </summary>
        /// <param name="newStatus">Nuevo estado.</param>
        /// <param name="changedByUserId">ID del usuario que realiza el cambio.</param>
        /// <param name="comment">Comentario opcional sobre el cambio.</param>
        public void ChangeStatus(string newStatus, int changedByUserId, string comment = null)
        {
            if (string.IsNullOrWhiteSpace(newStatus))
                throw new ArgumentException("El estado no puede estar vacío", nameof(newStatus));

            Status = newStatus;
            LastStatusChangedBy = changedByUserId;
            LastStatusChangeAt = DateTime.UtcNow;
            StatusComment = comment;
            MarkAsUpdated();
        }
    }
}
