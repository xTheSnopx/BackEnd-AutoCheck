using AutoCheckAML.Api.Entity.Base;

namespace AutoCheckAML.Api.Entity
{
    /// <summary>
    /// Historial de exportaciones (PDF, Excel) realizadas en el sistema.
    /// Para auditoría y control de acceso a datos.
    /// </summary>
    public class ExportHistory : AuditableEntity
    {
        /// <summary>
        /// ID del usuario que solicitó la exportación.
        /// </summary>
        public int ExportedByUserId { get; set; }

        /// <summary>
        /// Tipo de exportación: Excel, PDF, CSV
        /// </summary>
        public string ExportType { get; set; }

        /// <summary>
        /// Formato de archivo (xlsx, pdf, csv).
        /// </summary>
        public string FileFormat { get; set; }

        /// <summary>
        /// Entidad que fue exportada: FormSubmission, Crew, User, Report, etc.
        /// </summary>
        public string ExportedEntity { get; set; }

        /// <summary>
        /// Descripción o filtros aplicados en la exportación.
        /// </summary>
        public string FilterDescription { get; set; }

        /// <summary>
        /// Nombre del archivo generado.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Ruta o URI del archivo exportado.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Tamaño del archivo en bytes.
        /// </summary>
        public long FileSizeInBytes { get; set; }

        /// <summary>
        /// Número de registros incluidos en la exportación.
        /// </summary>
        public int RecordCount { get; set; }

        /// <summary>
        /// Duración de la exportación en milisegundos.
        /// </summary>
        public long DurationInMs { get; set; }

        /// <summary>
        /// Estado de la exportación: Success, Failed, Pending
        /// </summary>
        public string Status { get; set; } = "Success";

        /// <summary>
        /// Mensaje de error (si falló).
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Dirección IP desde donde se solicitó la exportación.
        /// </summary>
        public string IpAddress { get; set; }

        // Navigation properties
        public virtual User ExportedByUser { get; set; }
    }
}
