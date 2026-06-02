using AutoCheckAML.Api.Entity.Base;

namespace AutoCheckAML.Api.Entity
{
    /// <summary>
    /// Representa una cuadrilla (equipo de trabajo).
    /// Creada y administrada por el rol SOFTWARE (Administrador).
    /// </summary>
    public class Crew : AuditableEntity
    {
        /// <summary>
        /// Nombre único de la cuadrilla.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descripción de la cuadrilla y su función.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// ID del usuario SOFTWARE que gestiona esta cuadrilla.
        /// </summary>
        public int ManagedByUserId { get; set; }

        /// <summary>
        /// Departamento o área de la cuadrilla.
        /// Ejemplo: Mantenimiento, Operaciones, HSQ
        /// </summary>
        public string Department { get; set; }

        /// <summary>
        /// Ubicación o localización de la cuadrilla.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Número de miembros en la cuadrilla.
        /// </summary>
        public int MemberCount { get; set; }

        /// <summary>
        /// Indica si la cuadrilla está activa.
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual User ManagedByUser { get; set; }
        public virtual ICollection<User> Members { get; set; } = new List<User>();
        public virtual ICollection<FormSubmission> FormSubmissions { get; set; } = new List<FormSubmission>();
    }
}
