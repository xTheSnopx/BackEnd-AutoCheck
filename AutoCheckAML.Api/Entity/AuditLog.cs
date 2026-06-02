using AutoCheckAML.Api.Entity.Base;

namespace AutoCheckAML.Api.Entity
{
    /// <summary>
    /// Registro de auditoría del sistema.
    /// Rastrear quién hizo qué, cuándo y desde dónde.
    /// </summary>
    public class AuditLog : BaseEntity
    {
        /// <summary>
        /// ID del usuario que realizó la acción.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Entidad que fue modificada (ej: User, FormSubmission, Crew).
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// ID del registro de la entidad que fue modificado.
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Tipo de acción: Create, Update, Delete, Read, Login, Export, etc.
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Descripción detallada de la acción.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Valores anteriores (antes de la modificación) en formato JSON.
        /// </summary>
        public string OldValues { get; set; }

        /// <summary>
        /// Valores nuevos (después de la modificación) en formato JSON.
        /// </summary>
        public string NewValues { get; set; }

        /// <summary>
        /// Dirección IP desde donde se ejecutó la acción.
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// User Agent del cliente que ejecutó la acción.
        /// </summary>
        public string UserAgent { get; set; }

        // Navigation properties
        public virtual User User { get; set; }
    }
}
