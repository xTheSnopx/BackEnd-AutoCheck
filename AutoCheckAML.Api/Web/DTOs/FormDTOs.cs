using System.Collections.Generic;

namespace AutoCheckAML.Api.Web.DTOs
{
    // ========== FORM SUBMISSION DTOs (nueva arquitectura) ==========

    /// <summary>DTO de respuesta para un FormSubmission completo.</summary>
    public class FormSubmissionDto
    {
        public int Id { get; set; }
        public int FormTemplateId { get; set; }
        public string FormTemplateName { get; set; }
        public int SubmittedByUserId { get; set; }
        public string SubmittedByUserName { get; set; }
        public int? AssignedToCrewId { get; set; }
        public string AssignedToCrewName { get; set; }
        public string ActivityLocation { get; set; }
        public DateTime ActivityDate { get; set; }
        public string ObservationsByRespondent { get; set; }
        public string ObservationsByRectifier { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public int? VerifiedByUserId { get; set; }
        public string VerifiedByUserName { get; set; }
        public bool RequiresReview { get; set; }
        public bool RequiresClosure { get; set; }
        public string Status { get; set; }
        public List<AnswerDto> Answers { get; set; } = new List<AnswerDto>();
        public List<AttachmentDto> Attachments { get; set; } = new List<AttachmentDto>();
        public int? VehicleTypeId { get; set; }
        public string VehicleTypeName { get; set; }
        public DateTime CreatedAt { get; set; }

        // Approval fields (solo Ingeniero Mecánico aprueba)
        public int? ApprovedByIngenieroId { get; set; }
        public string ApprovedByIngenieroName { get; set; }
        public DateTime? ApprovedByIngenieroAt { get; set; }

        // HSEQ Review fields (revisión sin afectar estado)
        public int? ReviewedByHSEQId { get; set; }
        public string ReviewedByHSEQName { get; set; }
        public DateTime? ReviewedByHSEQAt { get; set; }
        public string ObservationsByHSEQ { get; set; }

        // DEPRECATED: Mantener por compatibilidad
        public int? ApprovedBySupervisorId { get; set; }
        public string ApprovedBySupervisorName { get; set; }
        public DateTime? ApprovedBySupervisorAt { get; set; }
    }

    /// <summary>Request para crear un nuevo FormSubmission.</summary>
    public class CreateFormSubmissionRequest
    {
        public int FormTemplateId { get; set; }
        public int? AssignedToCrewId { get; set; }
        public int? VehicleTypeId { get; set; }
        public string ActivityLocation { get; set; }
        public DateTime ActivityDate { get; set; }
        public string ObservationsByRespondent { get; set; }
        public string? PhotoData { get; set; }
        public List<string> Photos { get; set; } = new List<string>();
        public List<CreateAnswerRequest> Answers { get; set; } = new List<CreateAnswerRequest>();
    }

    /// <summary>Request para actualizar el estado de un formulario.</summary>
    public class UpdateFormSubmissionStatusRequest
    {
        public string Status { get; set; }
        public string Comment { get; set; }
    }

    /// <summary>Request para que la cuadrilla verifique el formulario.</summary>
    public class VerifyFormSubmissionRequest
    {
        public string ObservationsByRectifier { get; set; }
        public bool RequiresReview { get; set; }
    }

    /// <summary>Request para rechazar una inspección.</summary>
    public class RejectSubmissionRequest
    {
        public string Reason { get; set; }
    }

    /// <summary>Request para poner/quitar una inspección EN REVISIÓN.</summary>
    public class SetRevisionRequest
    {
        public bool InRevision { get; set; }
    }

    /// <summary>Request para registrar una revisión del Supervisor HSEQ.</summary>
    public class HSEQReviewRequest
    {
        public string Observations { get; set; }
    }

    // ========== FILTER / PAGING ==========

    public class FormSubmissionFilterRequest
    {
        public int? FormTemplateId { get; set; }
        public int? SubmittedByUserId { get; set; }
        public int? AssignedToCrewId { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? ActivityLocation { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    // ========== BACKWARD COMPAT: FormFilterRequest y StatusUpdateRequest ==========
    // Mantenidos para no romper código existente mientras se migra

    public class FormFilterRequest
    {
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class StatusUpdateRequest
    {
        public string Status { get; set; }
    }
}

