using System;

namespace AutoCheckAML.Api.Web.DTOs
{
    public class AnswerDto
    {
        public int Id { get; set; }
        public int FormSubmissionId { get; set; }
        public int FormFieldId { get; set; }
        public string FormFieldLabel { get; set; }
        public string FieldValue { get; set; }
        public string Notes { get; set; }
    }

    public class CreateAnswerRequest
    {
        public int FormFieldId { get; set; }
        public string FieldValue { get; set; }
        public string Notes { get; set; }
    }
}
