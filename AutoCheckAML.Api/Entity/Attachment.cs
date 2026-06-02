using AutoCheckAML.Api.Entity.Base;

namespace AutoCheckAML.Api.Entity
{
    /// <summary>
    /// Evidencia adjunta a una respuesta de formulario.
    /// Puede ser fotografías, documentos, certificados, etc.
    /// </summary>
    public class Attachment : AuditableEntity
    {
        /// <summary>
        /// ID del formulario respondido.
        /// </summary>
        public int FormSubmissionId { get; set; }

        /// <summary>
        /// ID del campo relacionado (opcional).
        /// </summary>
        public int? FormFieldId { get; set; }

        /// <summary>
        /// Nombre original del archivo.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Tipo MIME del archivo (image/jpeg, application/pdf, etc.).
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Tamaño del archivo en bytes.
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Ruta o URI del archivo almacenado.
        /// Puede ser local, en blob storage, S3, etc.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Descripción del archivo/evidencia.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Tipo de evidencia: Photograph, Document, Certificate, Signature, Other
        /// </summary>
        public string EvidenceType { get; set; }

        /// <summary>
        /// ID del usuario que subió la evidencia.
        /// </summary>
        public int UploadedByUserId { get; set; }

        // Navigation properties
        public virtual FormSubmission FormSubmission { get; set; }
        public virtual FormField FormField { get; set; }
        public virtual User UploadedByUser { get; set; }
    }
}
