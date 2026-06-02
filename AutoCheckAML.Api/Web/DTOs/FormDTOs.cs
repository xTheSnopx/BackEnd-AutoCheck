namespace AutoCheckAML.Api.Web.DTOs
{
    public class FormSubmissionRequest
    {
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Empresa { get; set; }
        public string Asunto { get; set; }
        public string Mensaje { get; set; }
        public DateTime Fecha { get; set; }
    }

    public class FormSubmissionResponse
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Empresa { get; set; }
        public string Asunto { get; set; }
        public string Mensaje { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }

    public class FormFilterRequest
    {
        public string SearchTerm { get; set; }
        public string Status { get; set; }
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
