using System;

namespace AutoCheckAML.Api.Web.DTOs
{
    public class ExportHistoryDto
    {
        public int Id { get; set; }
        public int ExportedByUserId { get; set; }
        public string ExportedByUserName { get; set; }
        public string ExportType { get; set; }
        public string FileFormat { get; set; }
        public string ExportedEntity { get; set; }
        public string FilterDescription { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public long FileSizeInBytes { get; set; }
        public int RecordCount { get; set; }
        public long DurationInMs { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
        public string IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
