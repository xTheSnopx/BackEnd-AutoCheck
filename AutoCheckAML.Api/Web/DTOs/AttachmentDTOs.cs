using System;

namespace AutoCheckAML.Api.Web.DTOs
{
    public class AttachmentDto
    {
        public int Id { get; set; }
        public int FormSubmissionId { get; set; }
        public int? FormFieldId { get; set; }
        public string FormFieldLabel { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public string FilePath { get; set; }
        /// <summary>Datos base64 para enviar al frontend.</summary>
        public string FileDataBase64 { get; set; }
        public string Description { get; set; }
        public string EvidenceType { get; set; }
        public int UploadedByUserId { get; set; }
        public string UploadedByUserName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateAttachmentRequest
    {
        public int? FormFieldId { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public string FilePath { get; set; }
        public string Description { get; set; }
        public string EvidenceType { get; set; }
    }
}
