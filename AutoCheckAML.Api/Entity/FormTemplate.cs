using AutoCheckAML.Api.Entity.Base;

namespace AutoCheckAML.Api.Entity
{
    /// <summary>
    /// Plantilla de formulario (template).
    /// Define la estructura de un formulario que puede ser respondido múltiples veces.
    /// </summary>
    public class FormTemplate : AuditableEntity
    {
        /// <summary>
        /// Nombre del formulario.
        /// Ejemplo: "Inspección Mecánica Diaria", "Checklist HSQ Semanal"
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descripción y propósito del formulario.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Tipo de formulario: Operativo, Mecánico, HSQ, Otro
        /// </summary>
        public string FormType { get; set; }

        /// <summary>
        /// Versión del formulario.
        /// </summary>
        public int Version { get; set; } = 1;

        /// <summary>
        /// Indica si el formulario está activo para uso.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Indica si el formulario requiere firma digital.
        /// </summary>
        public bool RequiresSignature { get; set; } = false;

        /// <summary>
        /// Orden de visualización del formulario.
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// ID del usuario que creó el formulario (generalmente SOFTWARE/Admin).
        /// </summary>
        public int CreatedByUserId { get; set; }

        // Navigation properties
        public virtual User CreatedByUser { get; set; }
        public virtual ICollection<FormField> FormFields { get; set; } = new List<FormField>();
        public virtual ICollection<FormSubmission> FormSubmissions { get; set; } = new List<FormSubmission>();
    }
}
