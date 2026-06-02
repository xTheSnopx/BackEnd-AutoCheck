using AutoCheckAML.Api.Entity.Base;

namespace AutoCheckAML.Api.Entity
{
    /// <summary>
    /// Respuesta a un campo específico de un formulario respondido.
    /// </summary>
    public class Answer : BaseEntity
    {
        /// <summary>
        /// ID de la respuesta del formulario (FormSubmission).
        /// </summary>
        public int FormSubmissionId { get; set; }

        /// <summary>
        /// ID del campo del formulario.
        /// </summary>
        public int FormFieldId { get; set; }

        /// <summary>
        /// Valor de la respuesta.
        /// </summary>
        public string FieldValue { get; set; }

        /// <summary>
        /// Notas o comentarios adicionales sobre esta respuesta.
        /// </summary>
        public string Notes { get; set; }

        // Navigation properties
        public virtual FormSubmission FormSubmission { get; set; }
        public virtual FormField FormField { get; set; }
    }
}
