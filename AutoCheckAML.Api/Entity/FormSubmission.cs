using AutoCheckAML.Api.Entity.Base;

namespace AutoCheckAML.Api.Entity
{
    /// <summary>
    /// Representa una respuesta/diligenciamiento de un formulario.
    /// Un usuario completa una plantilla de formulario y genera una FormSubmission.
    /// </summary>
    public class FormSubmission : StatusEntity
    {
        /// <summary>
        /// ID de la plantilla que se respondió.
        /// </summary>
        public int FormTemplateId { get; set; }

        /// <summary>
        /// ID del usuario que respondió el formulario (INGENIERO).
        /// </summary>
        public int SubmittedByUserId { get; set; }

        /// <summary>
        /// ID de la cuadrilla que verificará/rectificará (opcional).
        /// </summary>
        public int? AssignedToCrewId { get; set; }

        /// <summary>
        /// Ubicación/actividad a la que corresponde la respuesta.
        /// Ejemplo: "Máquina A", "Área de Ensamble"
        /// </summary>
        public string ActivityLocation { get; set; }

        /// <summary>
        /// Fecha de la actividad registrada.
        /// </summary>
        public DateTime ActivityDate { get; set; }

        /// <summary>
        /// Observaciones adicionales del respondedor.
        /// </summary>
        public string ObservationsByRespondent { get; set; } = string.Empty;

        /// <summary>
        /// Observaciones de la cuadrilla al verificar/rectificar.
        /// </summary>
        public string ObservationsByRectifier { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de verificación por la cuadrilla.
        /// </summary>
        public DateTime? VerifiedAt { get; set; }

        /// <summary>
        /// ID del usuario de la cuadrilla que verificó.
        /// </summary>
        public int? VerifiedByUserId { get; set; }

        /// <summary>
        /// Indica si el formulario requiere revisión adicional.
        /// </summary>
        public bool RequiresReview { get; set; } = false;

        /// <summary>
        /// ID del tipo de vehículo seleccionado (opcional).
        /// </summary>
        public int? VehicleTypeId { get; set; }

        /// <summary>
        /// Fecha de cierre del formulario.
        /// </summary>
        public DateTime? ClosedAt { get; set; }

        /// <summary>
        /// ID del usuario que cerró el formulario.
        /// </summary>
        public int? ClosedByUserId { get; set; }

        // ===== DOUBLE APPROVAL FIELDS =====
        /// <summary>
        /// ID del Ingeniero Mecánico que aprobó la inspección.
        /// </summary>
        public int? ApprovedByIngenieroId { get; set; }

        /// <summary>
        /// Fecha de aprobación por el Ingeniero Mecánico.
        /// </summary>
        public DateTime? ApprovedByIngenieroAt { get; set; }

        /// <summary>
        /// ID del Supervisor HSEQ que aprobó la inspección.
        /// </summary>
        public int? ApprovedBySupervisorId { get; set; }

        /// <summary>
        /// Fecha de aprobación por el Supervisor HSEQ.
        /// </summary>
        public DateTime? ApprovedBySupervisorAt { get; set; }

        // Navigation properties
        public virtual FormTemplate FormTemplate { get; set; }
        public virtual User SubmittedByUser { get; set; }
        public virtual User VerifiedByUser { get; set; }
        public virtual User ClosedByUser { get; set; }
        public virtual Crew AssignedToCrew { get; set; }
        public virtual VehicleType VehicleType { get; set; }
        public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
        public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}
